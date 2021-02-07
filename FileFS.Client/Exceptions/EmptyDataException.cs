using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when file data is empty.
    /// </summary>
    public class EmptyDataException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDataException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public EmptyDataException(string fileName)
            : base($"Data for file '{fileName}' must be non-emoty.")
        {
        }
    }
}