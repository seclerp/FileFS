using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when directory already exists in FileFS storage.
    /// </summary>
    public class DirectoryAlreadyExistsException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public DirectoryAlreadyExistsException(string fileName)
            : base($"Directory with name '{fileName}' already exists in FileFS storage.")
        {
        }
    }
}