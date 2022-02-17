using System;
using System.Threading.Tasks;

using ShinyBluetoothTest.EventArgs;

namespace ShinyBluetoothTest.Interfaces
{
    public interface ITransportLayer
    {
        event EventHandler<DeviceConnectedEventArgs> OnDeviceConnected;
        event EventHandler<DeviceDisconnectedEventArgs> OnDeviceDisconnected;
        event EventHandler<DataReceivedEventArgs> OnReceivedData;

        Task ConnectAsync();
        Task SendDataAsync(string id, byte[] data);
        bool IsDeviceConnected(string id);
        void DisconnectFromDevice(string id);
        void Disconnect();
    }
}
