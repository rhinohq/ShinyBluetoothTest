namespace ShinyBluetoothTest.EventArgs
{
    public class DeviceDisconnectedEventArgs
    {
        public DeviceDisconnectedEventArgs()
        {
        }

        public DeviceDisconnectedEventArgs(string id)
        {
            TransportId = id;
        }

        public string TransportId { get; set; }
    }
}
