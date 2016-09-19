using System;

namespace JsonCs
{
    /// <summary>
    /// An exception that is thrown when an unexpected character sequence is encountered,
    /// while reading JSON text.
    /// </summary>
    public sealed class UnexpectedJsonException : Exception
    {
    }
}