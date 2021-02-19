using FileFS.Client.Constants;
using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when name is invalid for FileFS storage.
    /// </summary>
    public class InvalidNameException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidNameException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public InvalidNameException(string fileName)
            : base($"'{fileName}' is invalid. Name must match pattern {PatternMatchingConstants.ValidFilename}.")
        {
        }
    }
}