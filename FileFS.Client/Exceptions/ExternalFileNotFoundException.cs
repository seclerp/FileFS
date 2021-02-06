using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when external file not found.
    /// </summary>
    public class ExternalFileNotFoundException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFileNotFoundException"/> class.
        /// </summary>
        /// <param name="path">Path to external file.</param>
        public ExternalFileNotFoundException(string path)
            : base($"File with path '{path}' not found in your filesystem.")
        {
        }
    }
}