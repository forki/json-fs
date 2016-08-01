using System.Globalization;

namespace ParserCs
{
    /// <summary>
    /// Builds a string, by performing a fast forward only read from a <see cref="CharStream"/>
    /// into an internal buffer. Any reads, will advance the reading position within the
    /// <see cref="CharStream"/>.
    /// </summary>
    public sealed class StringBuffer
    {
        public char[] Buffer { get; }
        public int BufferSize { get; }

        private StringBuffer(char[] buffer, int bufferSize)
        {
            Buffer = buffer;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Builds a <see cref="StringBuffer"/> by performing a fast forward only read from the
        /// given <see cref="CharStream"/>. Any characters between a set of quotation marks are 
        /// read into the internal buffer.
        /// </summary>
        /// <param name="stream">The <see cref="CharStream"/> to read from.</param>
        /// <returns>A new instance of a <see cref="StringBuffer"/>.</returns>
        public static StringBuffer FromStream(CharStream stream)
        {
            var buffer = new char[1024];
            var readPosition = 0;

            while (true)
            {
                if (stream.Peek() == '"' || stream.Peek() == CharStream.NullTerminator)
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

            return new StringBuffer(buffer, readPosition);
        }

        private static char ParseUnicode(CharStream stream)
        {
            // TODO: support reading n characters from the stream returns char[]
            var hex = new[] { stream.Read(), stream.Read(), stream.Read(), stream.Read() };

            return (char)int.Parse(new string(hex), NumberStyles.HexNumber);
        }

        public override string ToString() => new string(Buffer, 0, BufferSize);
    }
}
