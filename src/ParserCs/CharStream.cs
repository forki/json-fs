﻿using System;
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
        public CharStream(TextReader textReader)
        {
            _textReader = textReader;

            _buffer = new char[BufferLength];
            _buffer[_readPosition] = '\0';

            _textReader.Read(_buffer, 0, BufferLength);
        }

        public char Peek() => _buffer[_readPosition];

        public bool Skip(string toSkip)
        {
            if (string.IsNullOrEmpty(toSkip))
            {
                return true;
            }

            if (toSkip.Where((c, i) => _buffer[_readPosition + i] != c).Any())
            {
                return false;
            }

            _readPosition += toSkip.Length;
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
            while (_readPosition < BufferLength);
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
        private const int BufferLength = 1024;
        private int _readPosition;
        private bool _disposed;
    }
}
