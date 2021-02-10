using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when provided file data is null.
    /// </summary>
    public class DataIsNullException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataIsNullException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public DataIsNullException(string fileName)
            : base($"Data for file '{fileName}' must be not-null.")
        {
        }
    }
}