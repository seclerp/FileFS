namespace FileFS.Managers.Models
{
    public struct EntryInfo
    {
        public readonly string Path;

        public readonly int Size;

        public EntryInfo(string path, int size)
        {
            Path = path;
            Size = size;
        }
    }
}