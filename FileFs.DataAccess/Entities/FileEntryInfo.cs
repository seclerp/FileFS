namespace FileFs.DataAccess.Entities
{
    public struct FileEntryInfo
    {
        public readonly string FileName;

        public readonly int Size;

        public FileEntryInfo(string fileName, int size)
        {
            FileName = fileName;
            Size = size;
        }
    }
}