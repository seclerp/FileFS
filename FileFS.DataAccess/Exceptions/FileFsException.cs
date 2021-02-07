using System;

namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Base exception for all known FileFS exceptions.
    /// </summary>
    public abstract class FileFsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFsException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        protected FileFsException(string message)
            : base(message)
        {
        }
    }
}