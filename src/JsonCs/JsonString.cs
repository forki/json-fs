using System.Globalization;

namespace JsonCs
{
    /// <summary>
    /// Builds a string, by performing a fast forward only read from a <see cref="JsonStream"/>
    /// into an internal buffer. Any reads, will advance the reading position within the
    /// <see cref="JsonStream"/>.
    /// </summary>
    public sealed class JsonString
    {
        public JsonString()
        {
            _buffer = new char[DefaultBufferSize];
            _bufferSize = DefaultBufferSize;
        }

        /// <summary>
        /// Attempts to read any string character from the <see cref="JsonStream"/> in sequence, until the
        /// first end of string character is reached, or the stream is exhausted.
        /// </summary>
        /// <param name="stream">The <see cref="JsonStream"/> to read from.</param>
        /// <returns>
        /// The contents of the internal buffer as a string, as defined by the JSON 
        /// <a href="https://tools.ietf.org/html/rfc7159#section-8">RFC7591</a> specification.
        /// </returns>
        /// <exception cref="UnexpectedJsonException">
        /// The <see cref="JsonStream"/> contains an unexpected unicode escape character and can't
        /// be read any further. 
        /// </exception>
        public string Read(JsonStream stream)
        {
            _readPosition = 0;

            while (NotAtEndOfString(stream.Peek()))
            {
                if (EndOfBufferReached())
                {
                    ExpandBufferAndPreserveContent();
                }

                if (StartOfEscapeSequence(stream.Peek()))
                {
                    _buffer[_readPosition++] = ReadEscapeCharacter(stream);
                }
                else
                {
                    _buffer[_readPosition++] = stream.Read();
                }
            }

            return new string(_buffer, 0, _readPosition);
        }

        private static bool NotAtEndOfString(char character) => character != '"' && character != JsonStream.NullTerminator;

        private bool EndOfBufferReached() => _readPosition == _bufferSize;

        private void ExpandBufferAndPreserveContent()
        {
            var expandedBufferLength = _bufferSize*2;
            var expandedBuffer = new char[expandedBufferLength];

            Array.BlockCopy(_buffer, 0, expandedBuffer, 0, expandedBufferLength - _readPosition);

            _buffer = expandedBuffer;
            _bufferSize = expandedBufferLength;
        }

        private static bool StartOfEscapeSequence(char character) => character == EscapeCharacter;

        private static char ReadEscapeCharacter(JsonStream stream)
        {
            stream.Expect(EscapeCharacter);

            switch (stream.Read())
            {
                case '"':
                    return '\u0022';
                case '\\':
                    return '\u005c';
                case '/':
                    return '\u002f';
                case 'b':
                    return '\u0008';
                case 'f':
                    return '\u000c';
                case 'n':
                    return '\u000a';
                case 'r':
                    return '\u000d';
                case 't':
                    return '\u0009';
                case 'u':
                    return ParseUnicode(stream);
                default:
                    throw new UnexpectedJsonException();
            }
        }

        private static char ParseUnicode(JsonStream stream)
        {
            char unicodeCharacter;

            try
            {
                unicodeCharacter = (char) int.Parse(new string(stream.Read(4)), NumberStyles.HexNumber);
            }
            catch
            {
                throw new UnexpectedJsonException();
            }

            return unicodeCharacter;
        }

        private char[] _buffer;
        private int _readPosition;
        private int _bufferSize;

        private const int DefaultBufferSize = 1024;
        private const char EscapeCharacter = '\\';
    }
}
