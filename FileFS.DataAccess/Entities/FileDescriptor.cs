namespace FileFS.DataAccess.Entities
{
    public struct FileDescriptor
    {
        public static readonly int BytesWithoutFilename = 28;

        public FileDescriptor(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            FileNameLength = fileName.Length;
            FileName = fileName;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public readonly int FileNameLength;

        public readonly string FileName;

        public readonly long CreatedOn;

        public readonly long UpdatedOn;

        public readonly int DataOffset;

        public readonly int DataLength;
    }
}