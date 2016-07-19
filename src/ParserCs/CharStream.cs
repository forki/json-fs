using System;
using System.IO;
using System.Linq;

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
            _buffer[_readPosition] = '\0';

            _textReader.Read(_buffer, 0, _bufferSize);
        }

        public char Peek() => _buffer[_readPosition];

        public bool Skip(string characters)
        {
            if (string.IsNullOrEmpty(characters))
            {
                return true;
            }

            if (characters.Where((c, i) => _buffer[_readPosition + i] != c).Any())
            {
                return false;
            }

            _readPosition += characters.Length;
            return true;
        }

        public void SkipWhitespace()
        {
            do
            {
                if (char.IsWhiteSpace(_buffer[_readPosition]))
                {
                    _readPosition++;
                }
                else
                {
                    return;
                }
            }
            while (_readPosition < _bufferSize);
        }

        public char Read()
        {
            if (_readPosition != _bufferSize)
            {
                return _buffer[_readPosition++];
            }

            var charactersRead = _textReader.Read(_buffer, 0, _bufferSize);
            _readPosition = 0;

            if (charactersRead < _bufferSize)
            {
                _buffer[charactersRead] = '\0';
            }

            return _buffer[_readPosition++];
        }

        public char[] Read(uint length)
        {
            var characters = new char[length];

            for (var i = 0; i < length; i++)
            {
                characters[i] = Read();
            }

            return characters;
        }

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
        private const int DefaultBufferSize = 1024;
        private int _readPosition;
        private bool _disposed;
    }
}
