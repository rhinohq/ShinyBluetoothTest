using ShinyBluetoothTest.Interfaces;

namespace ShinyBluetoothTest.EventArgs
{
    public class DeviceConnectedEventArgs
    {
        public DeviceConnectedEventArgs()
        {
        }

        public DeviceConnectedEventArgs(string id)
        {
            TransportId = id;
        }

        public string TransportId { get; set; }
        public ITransportLayer TransportLayer { get; set; }
    }
}
