namespace FileFS.DataAccess.Exceptions
{
    public class EntryDescrisptorNotFound : FileFsException
    {
        public EntryDescrisptorNotFound(string entryName)
            : base($"Descriptor for entry {entryName} not found")
        {
        }
    }
}