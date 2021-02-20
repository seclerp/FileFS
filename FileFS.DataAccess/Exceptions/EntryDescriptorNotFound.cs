namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when there is no descriptor with given name.
    /// </summary>
    public class EntryDescriptorNotFound : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDescriptorNotFound"/> class.
        /// </summary>
        /// <param name="entryName">Name of an entry.</param>
        public EntryDescriptorNotFound(string entryName)
            : base($"Descriptor for entry {entryName} not found")
        {
        }
    }
}