using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Extensions
{
    public static class FilesystemDescriptorExtensions
    {
        public static FilesystemDescriptor WithFileDataLength(this in FilesystemDescriptor descriptor, int filesDataLength)
        {
            return new FilesystemDescriptor(
                filesDataLength,
                descriptor.FileDescriptorsCount,
                descriptor.FileDescriptorLength);
        }

        public static FilesystemDescriptor WithFileDescriptorsCount(this in FilesystemDescriptor descriptor, int fileDescriptorsCount)
        {
            return new FilesystemDescriptor(
                descriptor.FilesDataLength,
                fileDescriptorsCount,
                descriptor.FileDescriptorLength);
        }

        public static FilesystemDescriptor WithFileDescriptorLength(this in FilesystemDescriptor descriptor, int fileDescriptorLength)
        {
            return new FilesystemDescriptor(
                descriptor.FilesDataLength,
                descriptor.FileDescriptorsCount,
                fileDescriptorLength);
        }
    }
}