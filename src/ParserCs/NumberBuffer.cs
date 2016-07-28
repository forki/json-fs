namespace ParserCs
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NumberBuffer
    {
        public char[] Buffer { get; }
        public int BufferSize { get; }

        private NumberBuffer(char[] buffer, int bufferSize)
        {
            Buffer = buffer;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static NumberBuffer FromStream(CharStream stream)
        {
            var buffer = new char[1024];
            var readPosition = 0;

            while (true)
            {
                if (IsNumber(stream.Peek()))
                {
                    buffer[readPosition++] = stream.Read();
                }
                else
                {
                    break;
                }
            }

            return new NumberBuffer(buffer, readPosition);
        }

        private static bool IsNumber(char character)
        {
            switch (character)
            {
                case '-':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'e':
                case 'E':
                case '+':
                    return true;

                default:
                    return false;
            }
        }

        public override string ToString() => new string(Buffer, 0, BufferSize);
    }
}
