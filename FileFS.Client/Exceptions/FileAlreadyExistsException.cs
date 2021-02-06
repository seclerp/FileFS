using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when file already exists in FileFS storage.
    /// </summary>
    public class FileAlreadyExistsException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public FileAlreadyExistsException(string fileName)
            : base($"File with name '{fileName}' already exists in FileFS storage.")
        {
        }
    }
}