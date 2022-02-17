using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using ShinyBluetoothTest.EventArgs;
using ShinyBluetoothTest.Interfaces;
using ShinyBluetoothTest.Models;

namespace ShinyBluetoothTest.Services
{
    public class BluetoothService : ITransportLayer
    {
        readonly BluetoothClientService bluetoothClientService;
        readonly BluetoothHostingService bluetoothHostingService;

        public event EventHandler<DeviceConnectedEventArgs> OnDeviceConnected;
        public event EventHandler<DeviceDisconnectedEventArgs> OnDeviceDisconnected;
        public event EventHandler<DataReceivedEventArgs> OnReceivedData;

        public bool IsRunning =>
            bluetoothClientService.IsScanning &&
            bluetoothHostingService.IsAdvertising;

        public BluetoothService(BluetoothClientService clientService, BluetoothHostingService hostingService)
        {
            bluetoothClientService = clientService;
            bluetoothHostingService = hostingService;

            bluetoothClientService.OnDeviceConnected += ConnectDevice;
            bluetoothClientService.OnDeviceDisconnected += DisconnectDevice;
            bluetoothClientService.OnReceivedData += ReceiveData;

            bluetoothHostingService.OnReceivedData += ReceiveData;
        }

        public async Task ConnectAsync()
        {
            await bluetoothClientService.StartScanningAsync();
            await bluetoothHostingService.SetupServer();
        }

        public bool IsDeviceConnected(string id)
        {
            return bluetoothClientService.IsDeviceConnected(id) || bluetoothHostingService.IsDeviceConnected(id);
        }

        public Task SendDataAsync(string id, byte[] data)
        {
            if (UseClientOrHostingService(id))
            {
                return bluetoothClientService.SendDataAsync(id, data);
            }
            else
            {
                return bluetoothHostingService.SendDataAsync(id, data);
            }
        }

        public void DisconnectFromDevice(string id)
        {
            if (UseClientOrHostingService(id))
            {
                bluetoothClientService.DisconnectFromDevice(id);
            }
        }

        public void Disconnect()
        {
            bluetoothHostingService.ShutdownServer();
            bluetoothClientService.Disconnect();
        }

        /// <summary>Returns true if using Client Service.</summary>
        private bool UseClientOrHostingService(string id)
        {
            return bluetoothClientService.IsDeviceConnected(id);
        }

        private void ConnectDevice(object sender, DeviceConnectedEventArgs args)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                var req = new TestRequest() { Data = DateTime.Now.ToString(), Type = TestType.Test };

                Serializer.Serialize(stream, req);

                data = stream.ToArray();
            }

            SendDataAsync(args.TransportId, data).Wait();
        }

        private void DisconnectDevice(object sender, DeviceDisconnectedEventArgs args)
        {
            OnDeviceDisconnected?.Invoke(this, args);
        }

        private void ReceiveData(object sender, DataReceivedEventArgs args)
        {
            TestRequest meshMessage;

            using (var stream = new MemoryStream(args.Data))
            {
                meshMessage = Serializer.Deserialize<TestRequest>(stream);
            }
        }
    }
}
