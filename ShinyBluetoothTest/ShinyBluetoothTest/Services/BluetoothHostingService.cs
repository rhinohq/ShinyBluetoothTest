using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using ShinyBluetoothTest.EventArgs;
using ShinyBluetoothTest.Extensions;

using Shiny;
using Shiny.BluetoothLE.Hosting;

namespace ShinyBluetoothTest.Services
{
    public class BluetoothHostingService
    {
        public event EventHandler<DataReceivedEventArgs> OnReceivedData;

        private Dictionary<string, List<byte>> incomingMessages;

        readonly IBleHostingManager hostingManager;
        IGattCharacteristic dataCharacteristic;

        public bool IsAdvertising => hostingManager.IsAdvertising;

        public BluetoothHostingService(IBleHostingManager ble)
        {
            hostingManager = ble;

            incomingMessages = new Dictionary<string, List<byte>>();
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
            int endIndex = data.GetIndexOfTerminator();

            if (endIndex == -1) // Only a chunk of the full message
            {
                if (!incomingMessages.ContainsKey(senderId))
                    incomingMessages[senderId] = new List<byte>();

                incomingMessages[senderId].AddRange(data);
            }
            else
            {
                byte[] message = data[0..endIndex];

                if (incomingMessages.ContainsKey(senderId)) // Piece together with the other chunks of data we have received
                {
                    incomingMessages[senderId].AddRange(message);

                    message = incomingMessages[senderId].ToArray();
                }

                var args = new DataReceivedEventArgs(senderId, message);
                OnReceivedData?.Invoke(this, args);
            }
        }
    }
}
