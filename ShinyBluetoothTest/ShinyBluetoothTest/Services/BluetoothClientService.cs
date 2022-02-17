using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using ReactiveUI;

using ShinyBluetoothTest.EventArgs;
using ShinyBluetoothTest.Models;

using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace ShinyBluetoothTest.Services
{
    public class BluetoothClientService
    {
        public event EventHandler<DeviceConnectedEventArgs> OnDeviceConnected;
        public event EventHandler<DeviceDisconnectedEventArgs> OnDeviceDisconnected;
        public event EventHandler<DataReceivedEventArgs> OnReceivedData;

        readonly IBleManager bleManager;

        IManagedScan scanner;
        List<BluetoothDevice> btDevices;

        public bool IsScanning => bleManager != null && bleManager.IsScanning;

        public BluetoothClientService(IBleManager ble)
        {
            bleManager = ble;

            btDevices = new List<BluetoothDevice>();
        }

        public void Disconnect()
        {
            if (bleManager == null)
                return;

            StopScanning();

            foreach (var btDevice in btDevices)
            {
                btDevice.ManagedPeripheral.Dispose();
            }

            btDevices.Clear();
        }

        public async Task StartScanningAsync()
        {
            if (bleManager == null || bleManager.IsScanning)
                return;

            scanner = bleManager
                .CreateManagedScanner(RxApp.MainThreadScheduler, TimeSpan.FromSeconds(10),
                    new ScanConfig
                    {
                        ServiceUuids = { BluetoothConstants.PeripheralServiceId }
                    }
                );
            scanner.Peripherals.CollectionChanged += DeviceListChanged;

            await scanner.Start();
        }

        public void StopScanning()
        {
            if (scanner != null)
            {
                scanner.Stop();

                scanner = null;
            }
        }

        public bool IsDeviceConnected(string id)
        {
            var btDevice = btDevices.FirstOrDefault(x => x.Id == id);

            return btDevice != null && btDevice.ManagedPeripheral.Status == ConnectionState.Connected;
        }

        public async Task SendDataAsync(string id, byte[] data)
        {
            var btDevice = btDevices.FirstOrDefault(x => x.Id == id);

            if (btDevice != null && btDevice.ManagedPeripheral.Status == ConnectionState.Connected)
            {
                await btDevice.ManagedPeripheral.WriteBlob(
                    BluetoothConstants.PeripheralServiceId,
                    BluetoothConstants.DataCharacteristicId,
                    new MemoryStream(data)
                ).ToTask();
            }
        }

        public void DisconnectFromDevice(string id)
        {
            var btDevice = btDevices.FirstOrDefault(x => x.Id == id);

            btDevice.ManagedPeripheral.Dispose();
            btDevices.RemoveAll(x => x.Id == id);
        }

        private void ConnectToDevice(IPeripheral device)
        {
            var btDevice = btDevices.FirstOrDefault(x => x.Id == device.Uuid);

            if (btDevice != null)
                return;

            var managed = device.CreateManaged(RxApp.MainThreadScheduler);
            btDevice = new BluetoothDevice(managed);

            btDevices.Add(btDevice);

            btDevice.ConnectSub = managed.ConnectWait().Subscribe(peripheral =>
            {
                peripheral.Peripheral.GetKnownService(BluetoothConstants.PeripheralServiceId).Subscribe(service =>
                {
                    if (service == null)
                    {
                        IgnoreThisDevice();
                        return;
                    }

                    service.GetKnownCharacteristic(BluetoothConstants.DataCharacteristicId, throwIfNotFound: true).Subscribe(async characteristic =>
                    {
                        if (characteristic == null)
                        {
                            IgnoreThisDevice();
                            return;
                        }

                        try
                        {
                            // Test connection
                            var readData = await characteristic.ReadAsync();

                            await HookDataNotification(characteristic, btDevice);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);

                            IgnoreThisDevice();
                            return;
                        }

                        OnDeviceConnected?.Invoke(this, new DeviceConnectedEventArgs(btDevice.Id));
                    });
                });

                void IgnoreThisDevice()
                {
                    string thisUuid = peripheral.Peripheral.Uuid;

                    btDevices.RemoveAll(x => x.Id == thisUuid);
                }
            });
        }

        private async Task HookDataNotification(IGattCharacteristic gattCharacteristic, BluetoothDevice device)
        {
            await gattCharacteristic.EnableNotifications(true).ToTask();

            device.AddNotificationSub(gattCharacteristic
                .WhenNotificationReceived()
                .Subscribe(
                    result => {
                        var args = new DataReceivedEventArgs(result.Data);
                        args.SenderId = device.Id;

                        OnReceivedData?.Invoke(this, args);
                    }
                )
            );
        }

        private void DeviceListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in args.NewItems)
                {
                    var newDevice = (ManagedScanResult)newItem;

                    if (newDevice.IsConnectable.HasValue && !newDevice.IsConnectable.Value)
                        continue;

                    if (!newDevice.IsConnected)
                    {
                        ConnectToDevice(newDevice.Peripheral);
                    }
                } 
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in args.OldItems)
                {
                    var oldDevice = (ManagedScanResult)oldItem;

                    OnDeviceDisconnected?.Invoke(this, new DeviceDisconnectedEventArgs(oldDevice.Peripheral.Uuid));

                    btDevices.RemoveAll(x => x.Id == oldDevice.Peripheral.Uuid);
                }
            }
        }
    }
}
