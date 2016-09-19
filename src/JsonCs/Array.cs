using System;

namespace JsonCs
{
    internal static class Array
    {
        public static T[] Create<T>(int size, Func<T> initWith)
        {
            var array = new T[size];

            for (var i = 0; i < size; i++)
            {
                array[i] = initWith();
            }

            return array;
        }

        public static T[] Create<T>(int size, Func<T> initWith, Func<T, bool> stopWhen)
        {
            var array = new T[size];

            for (var i = 0; i < size; i++)
            {
                var value = initWith();

                if (stopWhen(value))
                {
                    break;
                }

                array[i] = value;
            }

            return array;
        }

        public static void BlockCopy(char[] source, int sourceOffset, char[] destination, int desintationOffset, int count)
        {
            const int charSizeBytes = 2;

            Buffer.BlockCopy(source, sourceOffset * charSizeBytes, destination, desintationOffset * charSizeBytes,
                count * charSizeBytes);
        }
    }
}
