using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when external file already exists.
    /// </summary>
    public class ExternalFileAlreadyExistsException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFileAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="path">Path to existing external file.</param>
        public ExternalFileAlreadyExistsException(string path)
            : base($"File with path '{path}' already exists in your filesystem.")
        {
        }
    }
}