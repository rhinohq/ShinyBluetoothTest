using ShinyBluetoothTest.Interfaces;

namespace ShinyBluetoothTest.EventArgs
{
    public class DataReceivedEventArgs
    {
        public DataReceivedEventArgs()
        {
        }

        public DataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        public DataReceivedEventArgs(string senderId, byte[] data)
        {
            SenderId = senderId;
            Data = data;
        }

        public byte[] Data { get; set; }
        public string SenderId { get; set; }
        public ITransportLayer TransportLayer { get; set; }
    }
}
