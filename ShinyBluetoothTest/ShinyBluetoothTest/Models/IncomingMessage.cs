using System.Collections.Generic;
using System.Linq;

namespace ShinyBluetoothTest.Models
{
    public class IncomingMessage
    {
        public int FullLength { get; set; }
        public List<byte> Data { get; set; }

        public IncomingMessage()
        {
        }

        public IncomingMessage(int length, byte[] data)
        {
            FullLength = length;
            Data = data.ToList();
        }
    }
}
