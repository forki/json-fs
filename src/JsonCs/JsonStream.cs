using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JsonCs
{
    /// <summary>
    /// A character stream that performs fast forward only reads and does not cache internally.
    /// Any form of backtracking is not supported.
    /// </summary>
    public class JsonStream : IDisposable
    {
        /// <summary>
        /// Constructs a new instance of a <see cref="JsonStream"/>.
        /// </summary>
        /// <param name="textReader">A reader that can read a sequential series of characters.</param>
        /// <param name="bufferSize">The size of the internal read buffer. Use to override the default.</param>
        /// <remarks>
        /// The default size of the internal read buffer is 1024 characters.
        /// </remarks>
        public JsonStream(TextReader textReader, int bufferSize = DefaultBufferSize)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            if (bufferSize < 0)
            {
                throw new ArgumentException("", nameof(bufferSize));
            }

            _textReader = textReader;
            _bufferSize = bufferSize;
            _buffer = new char[_bufferSize];

            FillBufferAndResetReadPosition();
        }

        private void FillBufferAndResetReadPosition()
        {
            _charactersInBuffer = _textReader.Read(_buffer, 0, _bufferSize);
            if (_charactersInBuffer < _bufferSize)
            {
                _buffer[_charactersInBuffer] = NullTerminator;
            }

            _readPosition = 0;
        }

        private void NullTerminateBufferIfNotAtCapacity(int charactersWritten)
        {
            if (charactersWritten < _bufferSize)
            {
                _buffer[charactersWritten] = NullTerminator;
            }
        }

        /// <summary>
        /// Peeks at the next character within the character stream, without advancing the
        /// reading position of the stream.
        /// </summary>
        /// <returns>The next character within the stream.</returns>
        public char Peek() => _buffer[_readPosition];

        /// <summary>
        /// Attempts to skip past a single character within the character stream.
        /// The reading position of the stream will only change if a successful match has
        /// occurred.
        /// </summary>
        /// <param name="character">The character to skip.</param>
        /// <returns>True if the character was skipped, otherwise false.</returns>
        /// <remarks>
        /// If skipping across a buffer boundary, the internal buffer will grow in size, to ensure
        /// the reading position can be reset on a failed match. Otherwise the original reading position
        /// will be lost during a buffer reload.
        /// </remarks>
        public bool Skip(char character)
        {
            if (BufferTerminated)
            {
                return false;
            }

            if (character != Peek())
            {
                return false;
            }

            _readPosition++;

            if (BufferEmpty)
            {
                FillBufferAndResetReadPosition();
            }

            return true;
        }

        /// <summary>
        /// Attempts to skip past a sequence of characters within the character stream.
        /// The reading position of the stream will only advance if a successful match has
        /// occurred.
        /// </summary>
        /// <param name="characters">The characters to skip.</param>
        /// <returns>True if the complete sequence of characters were skipped, otherwise false.</returns>
        /// <remarks>
        /// If skipping across a buffer boundary, the internal buffer will grow in size, to ensure
        /// the reading position can be reset on a failed match. Otherwise the original reading position
        /// will be lost during a buffer reload.
        /// </remarks>
        public bool Skip(string characters)
        {
            if (string.IsNullOrEmpty(characters))
            {
                return true;
            }

            if (BufferTerminated)
            {
                return false;
            }

            if (AvailableReads < characters.Length)
            {
                ExpandBufferAndPreserveContent(characters.Length);
            }

            var previousReadPosition = _readPosition;

            if (characters.All(c => c == ReadOne()))
            {
                if (BufferEmpty)
                {
                    FillBufferAndResetReadPosition();
                }

                return true;
            }

            _readPosition = previousReadPosition;
            return false;
        }

        private void ExpandBufferAndPreserveContent(int contentLength)
        {
            var expandedBufferLength = Math.Max(_bufferSize*2, contentLength);
            var expandedBuffer = new char[expandedBufferLength];

            Array.BlockCopy(_buffer, _readPosition, expandedBuffer, 0, _bufferSize - _readPosition);
            var charactersRead = _textReader.Read(expandedBuffer, _bufferSize, expandedBufferLength - _bufferSize);

            _buffer = expandedBuffer;
            _bufferSize = expandedBufferLength;

            NullTerminateBufferIfNotAtCapacity(_bufferSize + charactersRead);
        }

        /// <summary>
        /// Will attempt to skip all whitespace characters in the character stream,
        /// up until the next non-whitespace character.
        /// </summary>
        /// <remarks>
        /// A whitespace character is either, a space, a tab, a carriage return or a line feed.
        /// </remarks>
        public void SkipWhitespace()
        {
            if (AtNullTerminator())
            {
                return;
            }

            do
            {
                if (!char.IsWhiteSpace(Peek()))
                {
                    break;
                }

                _readPosition++;

                if (BufferEmpty)
                {
                    FillBufferAndResetReadPosition();
                }

            } while (BufferCanSeek);
        }

        private int AvailableReads => _charactersInBuffer - _readPosition;

        private bool BufferCanSeek => AvailableReads > 0;

        private bool BufferEmpty => AvailableReads == 0;

        private bool BufferTerminated => _buffer[_readPosition] == NullTerminator;

        /// <summary>
        /// Reads the next character from the character stream.
        /// </summary>
        /// <returns>
        /// The next character or <see cref="NullTerminator"/> if at the end of the stream.
        /// </returns>
        public char Read()
        {
            if (BufferTerminated)
            {
                return NullTerminator;
            }

            var character = ReadOne();

            if (BufferEmpty)
            {
                FillBufferAndResetReadPosition();
            }

            return character;
        }

        private char ReadOne() => _buffer[_readPosition++];

        private bool AtNullTerminator() => _buffer[_readPosition] == NullTerminator;

        private static bool AtNullTerminator(char character) => character == NullTerminator;

        /// <summary>
        /// Reads a number of characters from the character stream.
        /// </summary>
        /// <param name="expectedCharacters">The number of characters to be read.</param>
        /// <returns>An array of characters.</returns>
        /// <remarks>
        /// An array of a fixed size will always be returned by this method. If the number of
        /// <paramref name="expectedCharacters"/> could not have been read, the array will be
        /// padded with null terminator characters as required.
        /// </remarks>
        public char[] Read(int expectedCharacters)
        {
            char[] readCharacters;

            if (AvailableReads >= expectedCharacters)
            {
                readCharacters = Array.Create(expectedCharacters, ReadOne);
            }
            else
            {
                ExpandBufferAndPreserveContent(expectedCharacters);
                readCharacters = Array.Create(expectedCharacters, ReadOne, AtNullTerminator);
            }

            if (BufferEmpty)
            {
                FillBufferAndResetReadPosition();
            }

            return readCharacters;
        }

        /// <summary>
        /// Check that the next character within the stream is as expected.
        /// </summary>
        /// <param name="character">The expected character within the stream.</param>
        /// <exception cref="UnexpectedJsonException">
        /// The expected character did not appear within the <see cref="JsonStream"/> at the
        /// current read position.
        /// </exception>
        public void Expect(char character)
        {
            if (character != Read())
            {
                throw new UnexpectedJsonException();
            }
        }

        /// <summary>
        /// Attempts to read a property from the stream.
        /// </summary>
        /// <example>
        /// The property will be expected to be in the following format, as defined
        /// by the RFC [ADD LINK] specification.
        /// [ADD EXAMPLE]
        /// </example>
        /// <returns>The read property</returns>
        /// <exception cref="UnexpectedJsonException">
        /// [The stream was not in the expected format]
        /// </exception>
        public string ReadProperty()
        {
            if (BufferTerminated)
            {
                return "\0";
            }

            var property = ReadString();
            Expect(':');
            SkipWhitespace();

            return property;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadNumber()
        {
            SkipWhitespace();

            var readNumber = new StringBuilder();
            var canRead = true;
            do
            {
                var availableReads = AvailableReads;
                for (; availableReads > 0; availableReads--)
                {
                    if (IsNumber(Peek()))
                    {
                        readNumber.Append(_buffer[_readPosition++]);
                        continue;
                    }
                    
                    canRead = false;
                    break;
                }

                if (BufferEmpty)
                {
                    FillBufferAndResetReadPosition();
                }

            } while (canRead && AvailableReads > 0);

            SkipWhitespace();
            return readNumber.ToString();
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            if (BufferTerminated)
            {
                return "\0";
            }

            SkipWhitespace();
            Expect('"');

            var stringBuffer = new StringBuilder();
            var canRead = true;
            do
            {
                // TODO: This now seems to be a common approach to reading from the buffer
                // TODO: when jumping forward _readPosition++, the available reads count is out of sync (index out of bounds exception)
                var availableReads = AvailableReads;
                for (; availableReads > 0; availableReads--)
                {
                    // TODO: This logic is the only difference between other methods (could be captured in an Action)
                    var character = _buffer[_readPosition++];
                    if (character == '\\')
                    {
                        stringBuffer.Append(ReadEscapeCharacter());
                        availableReads--;
                    }
                    else if (character == '"' || character == '\0')
                    {
                        canRead = false;
                        break;
                    }
                    else
                    {
                        stringBuffer.Append(character);
                    }
                }

                if (BufferEmpty)
                {
                    FillBufferAndResetReadPosition();
                }

            } while (canRead && AvailableReads > 0);

            SkipWhitespace();
            return stringBuffer.ToString();
        }

        private char ReadEscapeCharacter()
        {
            switch (_buffer[_readPosition++])
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
                    return ParseUnicode();
                default:
                    throw new UnexpectedJsonException();
            }
        }

        private char ParseUnicode()
        {
            char unicodeCharacter;

            try
            {
                // TODO: Determine if 4 characters are available without the need to expand the buffer?

                unicodeCharacter = Convert.ToChar(int.Parse(new string(Read(4)), NumberStyles.HexNumber));
            }
            catch
            {
                throw new UnexpectedJsonException();
            }

            return unicodeCharacter;
        }

        /// <summary>
        /// Frees all underlying resources and closes the stream.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _textReader.Close();
            }
        }

        private readonly TextReader _textReader;
        private char[] _buffer;
        private int _bufferSize;
        private int _charactersInBuffer;
        private int _readPosition;

        private const int DefaultBufferSize = 1024;
        public const char NullTerminator = '\0';
    }
}
