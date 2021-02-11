using System;

namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when there is an error accessing existing FileFS storage.
    /// </summary>
    public class StorageNotAvailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageNotAvailableException"/> class.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="numOfAttempts">Number of attempts tried before error.</param>
        public StorageNotAvailableException(string fileFsStoragePath, int numOfAttempts)
            : base($"Cannot access storage located at '{fileFsStoragePath}', after {numOfAttempts} attempts.")
        {
        }
    }
}