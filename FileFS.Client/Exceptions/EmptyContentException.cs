using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when file content is empty.
    /// </summary>
    public class EmptyContentException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyContentException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public EmptyContentException(string fileName)
            : base($"Content for file '{fileName}' must be non-emoty.")
        {
        }
    }
}