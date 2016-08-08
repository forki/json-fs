namespace ParserCs
{
    /// <summary>
    /// Builds a number, by performing a fast forward only read from a <see cref="JsonStream"/>
    /// into an internal buffer. Any reads, will advance the reading position within the
    /// <see cref="JsonStream"/>.
    /// </summary>
    public sealed class JsonNumber
    {
        public char[] Buffer { get; }
        public int BufferSize { get; }

        private JsonNumber(char[] buffer, int bufferSize)
        {
            Buffer = buffer;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static JsonNumber FromStream(JsonStream stream)
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

            return new JsonNumber(buffer, readPosition);
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
