using System;
using System.IO;

namespace ParserCs
{
    /// <summary>
    /// A character stream that performs fast forward only reads and does not cache internally.
    /// Any form of backtracking is not supported.
    /// </summary>
    public class CharStream : IDisposable
    {
        /// <summary>
        /// Constructs a new instance of a <see cref="CharStream"/>.
        /// </summary>
        /// <param name="textReader">A reader that can read a sequential series of characters.</param>
        /// <param name="bufferSize">The size of the internal read buffer. Use to override the default.</param>
        /// <remarks>
        /// The default size of the internal read buffer is 1024 characters.
        /// </remarks>
        public CharStream(TextReader textReader, int bufferSize = DefaultBufferSize)
        {
            _textReader = textReader;
            _bufferSize = bufferSize;
            _buffer = new char[_bufferSize];

            FillBufferAndResetReadPosition();
        }

        private void FillBufferAndResetReadPosition()
        {
            var charactersRead = _textReader.Read(_buffer, 0, _bufferSize);

            _readPosition = 0;

            if (charactersRead < _bufferSize)
            {
                _buffer[charactersRead] = NullTerminator;
            }
        }

        public char Peek() => _buffer[_readPosition];

        public bool Skip(string characters)
        {
            if (_buffer[_readPosition] == NullTerminator)
            {
                return false;
            }

            if (string.IsNullOrEmpty(characters))
            {
                return true;
            }

            var readPositionOffset = 0;

            foreach (var c in characters)
            {
                if (AtEndOfBuffer(readPositionOffset))
                {
                    FillBufferAndResetReadPosition();
                    readPositionOffset = 0;
                }

                if (_buffer[_readPosition + readPositionOffset] != c)
                {
                    return false;
                }

                readPositionOffset++;
            }

            _readPosition += readPositionOffset;
            return true;
        }

        private bool AtEndOfBuffer(int position) => position == _bufferSize;

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

        public char Read()
        {
            if (CanReadFromBuffer())
            {
                if (_buffer[_readPosition] == NullTerminator)
                {
                    return NullTerminator;
                }
                else
                {
                    return _buffer[_readPosition++];
                }
            }

            FillBufferAndResetReadPosition();

            if (_buffer[_readPosition] == NullTerminator)
            {
                return NullTerminator;
            }

            return _buffer[_readPosition++];
        }

        private bool CanReadFromBuffer() => _readPosition < _bufferSize;

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
        private readonly char[] _buffer;
        private readonly int _bufferSize;
        private int _readPosition;
        private bool _disposed;

        private const int DefaultBufferSize = 1024;
        private const char NullTerminator = '\0';
    }
}
