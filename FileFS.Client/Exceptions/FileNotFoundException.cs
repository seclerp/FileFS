using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when file not found in FileFS storage.
    /// </summary>
    public class FileNotFoundException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileNotFoundException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public FileNotFoundException(string fileName)
            : base($"File with name '{fileName}' not found in FileFS storage.")
        {
        }
    }
}