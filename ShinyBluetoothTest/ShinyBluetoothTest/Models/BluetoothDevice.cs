using System;
using System.Collections.Generic;

using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Managed;

namespace ShinyBluetoothTest.Models
{
    public class BluetoothDevice
    {
        public IManagedPeripheral ManagedPeripheral { get; set; }

        public IDisposable ConnectSub { get; set; }
        public List<IDisposable> NotificationSubs { get; private set; }

        public string Id => ManagedPeripheral?.Peripheral.Uuid;
        public bool IsConnected => ManagedPeripheral != null ? ManagedPeripheral.Peripheral.IsConnected() : false;

        public BluetoothDevice()
        {
            NotificationSubs = new List<IDisposable>();
        }

        public BluetoothDevice(IManagedPeripheral managedPeripheral)
        {
            NotificationSubs = new List<IDisposable>();

            ManagedPeripheral = managedPeripheral;
        }

        public void AddNotificationSub(IDisposable notificationSub)
        {
            if (notificationSub == null)
                NotificationSubs = new List<IDisposable>() { notificationSub };
            else
                NotificationSubs.Add(notificationSub);
        }
    }
}
