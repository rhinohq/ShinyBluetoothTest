using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using ShinyBluetoothTest.EventArgs;

using Shiny;
using Shiny.BluetoothLE.Hosting;

namespace ShinyBluetoothTest.Services
{
    public class BluetoothHostingService
    {
        public event EventHandler<DataReceivedEventArgs> OnReceivedData;

        readonly IBleHostingManager hostingManager;
        IGattCharacteristic dataCharacteristic;

        public bool IsAdvertising => hostingManager.IsAdvertising;

        public BluetoothHostingService(IBleHostingManager ble)
        {
            hostingManager = ble;
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
                    LocalName = "RKendrickChat",
                    AndroidIncludeDeviceName = true,
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
                        var args = new DataReceivedEventArgs(request.Data);
                        args.SenderId = request.Peripheral.Uuid;

                        OnReceivedData?.Invoke(this, args);

                        return GattState.Success;
                    });

                    cb.SetRead(request => ReadResult.Success(new byte[0]));
                    cb.SetNotification();
                }
            );
        }
    }
}
