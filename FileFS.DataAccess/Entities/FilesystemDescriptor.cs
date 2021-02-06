namespace FileFS.DataAccess.Entities
{
    public struct FilesystemDescriptor
    {
        public static readonly int BytesTotal = 12;

        public FilesystemDescriptor(int filesDataLength, int fileDescriptorsCount, int fileDescriptorLength)
        {
            FilesDataLength = filesDataLength;
            FileDescriptorsCount = fileDescriptorsCount;
            FileDescriptorLength = fileDescriptorLength;
        }

        public readonly int FilesDataLength;

        public readonly int FileDescriptorsCount;

        public readonly int FileDescriptorLength;
    }
}