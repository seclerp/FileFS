namespace FileFs.DataAccess.Entities
{
    public struct FileDescriptor
    {
        public static readonly int BytesWithoutFilename = 12;

        public FileDescriptor(string fileName, int dataOffset, int dataLength)
        {
            FileNameLength = fileName.Length;
            FileName = fileName;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public readonly int FileNameLength;

        public readonly string FileName;

        public readonly int DataOffset;

        public readonly int DataLength;
    }
}