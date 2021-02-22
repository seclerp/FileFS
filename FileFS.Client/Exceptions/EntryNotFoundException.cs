using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when entry not found in FileFS storage.
    /// </summary>
    public class EntryNotFoundException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntryNotFoundException"/> class.
        /// </summary>
        /// <param name="name">Name of an entry.</param>
        public EntryNotFoundException(string name)
            : base($"Entry with name '{name}' not found in FileFS storage.")
        {
        }
    }
}