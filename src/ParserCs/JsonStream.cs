using System;
using System.IO;
using System.Linq;

namespace ParserCs
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
        /// <exception cref=""></exception>
        /// <remarks>
        /// The default size of the internal read buffer is 1024 characters.
        /// </remarks>
        public JsonStream(TextReader textReader, int bufferSize = DefaultBufferSize)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            _textReader = textReader;
            _bufferSize = bufferSize;
            _buffer = new char[_bufferSize];

            FillBufferAndResetReadPosition();
        }

        private void FillBufferAndResetReadPosition()
        {
            var charactersRead = _textReader.Read(_buffer, 0, _bufferSize);
            NullTerminateBufferIfNotAtCapacity(charactersRead);

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
            if (AtNullTerminator())
            {
                return false;
            }

            if (EndOfBufferReached())
            {
                ExpandBufferAndPreserveContent(1, _readPosition);
            }

            if (_buffer[_readPosition] != character)
            {
                return false;
            }

            _readPosition++;
            return true;
        }

        /// <summary>
        /// Attempts to skip past a sequence of characters within the character stream.
        /// The reading position of the stream will only change if a successful match has
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
            if (AtNullTerminator())
            {
                return false;
            }

            if (string.IsNullOrEmpty(characters))
            {
                return true;
            }

            var previousReadPosition = _readPosition;

            foreach (var c in characters)
            {
                if (EndOfBufferReached())
                {
                    var readSoFar = _readPosition - previousReadPosition;

                    ExpandBufferAndPreserveContent(characters.Length, readSoFar);
                }

                if (_buffer[_readPosition] != c)
                {
                    _readPosition = previousReadPosition;
                    return false;
                }

                _readPosition++;
            }

            return true;
        }

        private void ExpandBufferAndPreserveContent(int contentLength, int readSoFar)
        {
            var expandedBufferLength = Math.Max(_bufferSize*2, contentLength);
            var expandedBuffer = new char[expandedBufferLength];

            BlockCopy(_buffer, _readPosition - readSoFar, expandedBuffer, 0, readSoFar);
            var charactersRead = _textReader.Read(expandedBuffer, _readPosition, expandedBufferLength - _readPosition);

            _buffer = expandedBuffer;
            _bufferSize = expandedBufferLength;

            NullTerminateBufferIfNotAtCapacity(_readPosition + charactersRead);
        }

        private static void BlockCopy(char[] source, int sourceOffset, char[] destination, int desintationOffset, int count)
        {
            const int charSizeBytes = 2;

            Buffer.BlockCopy(source, sourceOffset * charSizeBytes, destination, desintationOffset * charSizeBytes, 
                count * charSizeBytes);
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
            while (char.IsWhiteSpace(_buffer[_readPosition]))
            {
                _readPosition++;

                if (EndOfBufferReached())
                {
                    FillBufferAndResetReadPosition();
                }
            }
        }

        private bool EndOfBufferReached() => _readPosition == _bufferSize;

        /// <summary>
        /// Reads the next character from the character stream.
        /// </summary>
        /// <returns>
        /// The next character or <see cref="NullTerminator"/> if at the end of the stream.
        /// </returns>
        public char Read()
        {
            if (CanReadFromBuffer())
            {
                return CheckAndReadCharacterFromBuffer();
            }

            FillBufferAndResetReadPosition();

            return CheckAndReadCharacterFromBuffer();
        }

        private bool CanReadFromBuffer() => _readPosition < _bufferSize;

        private char CheckAndReadCharacterFromBuffer() => AtNullTerminator() ? NullTerminator : _buffer[_readPosition++];

        private bool AtNullTerminator() => _buffer[_readPosition] == NullTerminator;

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
            var characters = new char[expectedCharacters];

            for (var i = 0; i < expectedCharacters; i++)
            {
                characters[i] = Read();
            }
     
            return characters;
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
            var readCharacter = CheckAndReadCharacterFromBuffer();

            if (character != readCharacter)
            {
                throw new UnexpectedJsonException();
            }
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
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _textReader.Close();
            }

            _disposed = true;
        }

        private readonly TextReader _textReader;
        private char[] _buffer;
        private int _bufferSize;
        private int _readPosition;
        private bool _disposed;

        private const int DefaultBufferSize = 1024;
        public const char NullTerminator = '\0';
    }
}
