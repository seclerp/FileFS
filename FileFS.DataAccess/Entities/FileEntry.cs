namespace FileFS.DataAccess.Entities
{
    public struct FileEntry
    {
        public readonly string FileName;

        public readonly byte[] Content;

        public FileEntry(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}