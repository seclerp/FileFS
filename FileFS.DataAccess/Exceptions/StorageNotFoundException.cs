using System;

namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when storage not exists.
    /// </summary>
    public class StorageNotFoundException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public StorageNotFoundException(string message)
            : base(message)
        {
        }
    }
}