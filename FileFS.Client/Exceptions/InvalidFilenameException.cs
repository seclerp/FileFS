using FileFS.Client.Constants;
using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when filename is invalid for FileFS storage.
    /// </summary>
    public class InvalidFilenameException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFilenameException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public InvalidFilenameException(string fileName)
            : base($"'{fileName}' is invalid. Filename must match pattern {PatternMatching.ValidFilename}.")
        {
        }
    }
}