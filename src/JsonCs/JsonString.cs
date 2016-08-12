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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedJsonException">
        /// </exception>
        public string Read(JsonStream stream)
        {
            _readPosition = 0;

            while (true)
            {
                if (EndOfStringReached(stream.Peek()))
                {
                    break;
                }

                if (IsEscapedString(stream.Peek()))
                {
                    stream.Read();
                    char escapedCharacter;

                    switch (stream.Read())
                    {
                        case '"':
                            escapedCharacter = '\u0022';
                            break;
                        case '\\':
                            escapedCharacter = '\u005c';
                            break;
                        case '/':
                            escapedCharacter = '\u002f';
                            break;
                        case 'b':
                            escapedCharacter = '\u0008';
                            break;
                        case 'f':
                            escapedCharacter = '\u000c';
                            break;
                        case 'n':
                            escapedCharacter = '\u000a';
                            break;
                        case 'r':
                            escapedCharacter = '\u000d';
                            break;
                        case 't':
                            escapedCharacter = '\u0009';
                            break;
                        case 'u':
                            escapedCharacter = ParseUnicode(stream);
                            break;
                        default:
                            throw new UnexpectedJsonException();
                    }

                    _buffer[_readPosition++] = escapedCharacter;
                }
                else
                {
                    _buffer[_readPosition++] = stream.Read();
                }
            }

            return new string(_buffer, 0, _readPosition);
        }

        private static bool EndOfStringReached(char character) => character == '"' || character == JsonStream.NullTerminator;

        private static bool IsEscapedString(char character) => character == '\\';

        private static char ParseUnicode(JsonStream stream)
        {
            try
            {
                return (char) int.Parse(new string(stream.Read(4)), NumberStyles.HexNumber);
            }
            catch
            {
                throw new UnexpectedJsonException();
            }
        }

        private readonly char[] _buffer;
        private int _readPosition;

        private const int DefaultBufferSize = 1024;
    }
}
