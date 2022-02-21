using System;

namespace ShinyBluetoothTest.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] AddLengthHeader(this byte[] data)
        {
            var header = data.GetLengthHeader();
            var messageFrame = new byte[data.Length + 4];

            for (int i = 0; i < 4; i++)
            {
                messageFrame[i] = header[i];
            }

            for (int i = 4; i < messageFrame.Length; i++)
            {
                messageFrame[i] = data[i - 4];
            }

            return messageFrame;
        }

        public static byte[] GetLengthHeader(this byte[] data)
        {
            var header = BitConverter.GetBytes(data.Length);

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);

            return header;
        }

        public static int ParseMessageLength(this byte[] messageFrame)
        {
            var header = messageFrame[..4];

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);

            int length = BitConverter.ToInt32(header, 0);

            return length;
        }
    }
}
