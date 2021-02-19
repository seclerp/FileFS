using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when file not found in FileFS storage.
    /// </summary>
    public class DirectoryNotFoundException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryNotFoundException"/> class.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        public DirectoryNotFoundException(string fileName)
            : base($"Directory with name '{fileName}' not found in FileFS storage.")
        {
        }
    }
}