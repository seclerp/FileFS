namespace FileFS.DataAccess.Entities
{
    public struct FilesystemDescriptor
    {
        public static readonly int BytesTotal = 16;

        public FilesystemDescriptor(int filesDataLength, int fileDescriptorsCount, int fileDescriptorLength, int version)
        {
            FilesDataLength = filesDataLength;
            FileDescriptorsCount = fileDescriptorsCount;
            FileDescriptorLength = fileDescriptorLength;
            Version = version;
        }

        public readonly int FilesDataLength;

        public readonly int FileDescriptorsCount;

        public readonly int FileDescriptorLength;

        public readonly int Version;
    }
}