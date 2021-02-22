using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when entry already exists in FileFS storage.
    /// </summary>
    public class EntryAlreadyExistsException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntryAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="name">Name of an entry.</param>
        public EntryAlreadyExistsException(string name)
            : base($"File with name '{name}' already exists in FileFS storage.")
        {
        }
    }
}