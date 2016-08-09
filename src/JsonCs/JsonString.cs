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
        public char[] Buffer { get; }
        public int BufferSize { get; }

        private JsonString(char[] buffer, int bufferSize)
        {
            Buffer = buffer;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Builds a <see cref="JsonString"/> by performing a fast forward only read from the
        /// given <see cref="JsonStream"/>. Any characters between a set of quotation marks are 
        /// read into the internal buffer.
        /// </summary>
        /// <param name="stream">The <see cref="JsonStream"/> to read from.</param>
        /// <returns>A new instance of a <see cref="JsonString"/>.</returns>
        public static JsonString FromStream(JsonStream stream)
        {
            var buffer = new char[1024];
            var readPosition = 0;

            while (true)
            {
                if (stream.Peek() == '"' || stream.Peek() == JsonStream.NullTerminator)
                {
                    break;
                }

                if (stream.Peek() == '\\')
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
                            // TODO: break out and stop writing to the buffer
                            escapedCharacter = ' ';
                            break;
                    }

                    buffer[readPosition++] = escapedCharacter;
                }
                else
                {
                    buffer[readPosition++] = stream.Read();
                }
            }

            return new JsonString(buffer, readPosition);
        }

        private static char ParseUnicode(JsonStream stream)
        {
            return (char)int.Parse(new string(stream.Read(4)), NumberStyles.HexNumber);
        }

        public override string ToString() => new string(Buffer, 0, BufferSize);
    }
}
