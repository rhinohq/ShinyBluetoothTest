namespace ShinyBluetoothTest.Extensions
{
    public static class ByteArrayExtensions
    {
        public static int GetIndexOfTerminator(this byte[] data)
        {
            if (data.Length <= 2) // Cannot find terminator
                return data.Length;

            byte lastValue = data[0];

            for (int i = 1; i < data.Length; i++)
            {
                var currentValue = data[i];

                if (currentValue == 0 && lastValue == 0)
                    return i - 1;

                lastValue = currentValue;
            }

            return -1; // No terminator in this data
        }
    }
}
