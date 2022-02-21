using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using ShinyBluetoothTest.EventArgs;
using ShinyBluetoothTest.Extensions;
using ShinyBluetoothTest.Models;

using Shiny;
using Shiny.BluetoothLE.Hosting;

namespace ShinyBluetoothTest.Services
{
    public class BluetoothHostingService
    {
        public event EventHandler<DataReceivedEventArgs> OnReceivedData;

        private Dictionary<string, IncomingMessage> incomingMessages;

        readonly IBleHostingManager hostingManager;
        IGattCharacteristic dataCharacteristic;

        public bool IsAdvertising => hostingManager.IsAdvertising;

        public BluetoothHostingService(IBleHostingManager ble)
        {
            hostingManager = ble;

            incomingMessages = new Dictionary<string, IncomingMessage>();
        }

        public async Task SetupServer()
        {
            if (hostingManager.IsAdvertising || hostingManager.Status != AccessState.Available)
                return;

            hostingManager.ClearServices();

            await hostingManager.AddService(
                BluetoothConstants.PeripheralServiceId,
                true,
                BuildService
            );

            try
            {
                await hostingManager.StartAdvertising(new AdvertisementOptions
                {
                    UseGattServiceUuids = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ShutdownServer()
        {
            if (hostingManager.IsAdvertising)
            {
                try
                {
                    hostingManager.StopAdvertising();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public bool IsDeviceConnected(string id)
        {
            var device = dataCharacteristic.SubscribedCentrals.FirstOrDefault(x => x.Uuid == id);

            return device != null;
        }

        public async Task SendDataAsync(string id, byte[] data)
        {
            var device = dataCharacteristic.SubscribedCentrals.FirstOrDefault(x => x.Uuid == id);

            if (device == null)
                return;

            await dataCharacteristic.Notify(data, device);
        }

        private void BuildService(IGattServiceBuilder serviceBuilder)
        {
            this.dataCharacteristic = serviceBuilder.AddCharacteristic(
                BluetoothConstants.DataCharacteristicId,
                cb =>
                {
                    cb.SetWrite(request =>
                    {
                        byte[] data;

                        if (request.Offset > 0)
                            data = request.Data[request.Offset..request.Data.Length];
                        else
                            data = request.Data;

                        ReceiveData(request.Peripheral.Uuid, data);

                        return GattState.Success;
                    });

                    cb.SetRead(request => ReadResult.Success(new byte[0]));
                    cb.SetNotification();
                }
            );
        }

        private void ReceiveData(string senderId, byte[] data)
        {
            if (incomingMessages.ContainsKey(senderId)) // Incoming message
            {
                ProcessIncomingMessage(senderId, data);
            }
            else // New Message
            {
                ProcessNewMessage(senderId, data);
            }
        }

        private void ProcessNewMessage(string senderId, byte[] data)
        {
            int messageLength = data.ParseMessageLength();
            byte[] frameData = data[4..];

            if (frameData.Length == messageLength)
            {
                SubmitData(senderId, frameData);
            }
            else if (frameData.Length > messageLength)
            {
                SubmitData(senderId, frameData[..messageLength]);
            }
            else // We have not received the full message
            {
                var message = new IncomingMessage(messageLength, frameData);

                incomingMessages.Add(senderId, message);
            }
        }

        private void ProcessIncomingMessage(string senderId, byte[] data)
        {
            var incomingMessage = incomingMessages[senderId];
            var collectedDataSize = incomingMessage.Data.Count + data.Length;

            if (incomingMessage.FullLength > collectedDataSize) // We don't have all of the message yet
            {
                incomingMessage.Data.AddRange(data);
            }
            else // We have all of the message
            {
                if (incomingMessage.FullLength == collectedDataSize)
                {
                    incomingMessage.Data.AddRange(data);
                }
                else // We have collected more data than needed so trim
                {
                    var excessAmountOfDataInFrame = collectedDataSize - incomingMessage.FullLength;

                    incomingMessage.Data.AddRange(data[..^excessAmountOfDataInFrame]);
                }

                SubmitData(senderId, incomingMessage.Data.ToArray());
            }
        }

        private void SubmitData(string senderId, byte[] submissionData)
        {
            var args = new DataReceivedEventArgs(senderId, submissionData);
            OnReceivedData?.Invoke(this, args);
        }
    }
}

