namespace JsonCs
{
    /// <summary>
    /// Builds a number, by performing a fast forward only read from a <see cref="JsonStream"/>
    /// into an internal buffer. Any reads, will advance the reading position within the
    /// <see cref="JsonStream"/>.
    /// </summary>
    public sealed class JsonNumber
    {
        public JsonNumber()
        {
            _buffer = new char[DefaultBufferSize];
        }

        /// <summary>
        /// Attempts to read any numeric character from the <see cref="JsonStream"/> in sequence, until the
        /// first non-numeric character is encountered.
        /// </summary>
        /// <param name="stream">The <see cref="JsonStream"/> to read from.</param>
        /// <returns>
        /// A string representation of the internal buffer, which contains a number as defined by the JSON 
        /// <a href="https://tools.ietf.org/html/rfc7159#section-6">RFC7591</a> specification.
        /// </returns>
        /// <remarks>
        /// No form of validation is carried out against the numeric sequence before returning it.
        /// </remarks>
        public string Read(JsonStream stream)
        {
            _readPosition = 0;

            while (true)
            {
                if (IsNumber(stream.Peek()))
                {
                    _buffer[_readPosition++] = stream.Read();
                }
                else
                {
                    break;
                }
            }

            return new string(_buffer, 0, _readPosition);
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

        private readonly char[] _buffer;
        private int _readPosition;

        private const int DefaultBufferSize = 1024;
    }
}
