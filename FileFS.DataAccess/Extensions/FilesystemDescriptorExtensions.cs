using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Extensions
{
    /// <summary>
    /// Extensions for <see cref="FilesystemDescriptor"/> instance.
    /// </summary>
    public static class FilesystemDescriptorExtensions
    {
        /// <summary>
        /// Returns new <see cref="FilesystemDescriptor"/> instance with FileDataLength set to new value.
        /// </summary>
        /// <param name="descriptor">Instance of <see cref="FilesystemDescriptor"/>.</param>
        /// <param name="filesDataLength">New value for FileDataLength.</param>
        /// <returns>New <see cref="FilesystemDescriptor"/> instance with FileDataLength set to new value.</returns>
        public static FilesystemDescriptor WithFileDataLength(this in FilesystemDescriptor descriptor, int filesDataLength)
        {
            return new FilesystemDescriptor(
                filesDataLength,
                descriptor.FileDescriptorsCount,
                descriptor.FileDescriptorLength);
        }

        /// <summary>
        /// Returns new <see cref="FilesystemDescriptor"/> instance with FileDescriptorsCount set to new value.
        /// </summary>
        /// <param name="descriptor">Instance of <see cref="FilesystemDescriptor"/>.</param>
        /// <param name="fileDescriptorsCount">New value for FileDescriptorsCount.</param>
        /// <returns>New <see cref="FilesystemDescriptor"/> instance with FileDescriptorsCount set to new value.</returns>
        public static FilesystemDescriptor WithFileDescriptorsCount(this in FilesystemDescriptor descriptor, int fileDescriptorsCount)
        {
            return new FilesystemDescriptor(
                descriptor.FilesDataLength,
                fileDescriptorsCount,
                descriptor.FileDescriptorLength);
        }

        /// <summary>
        /// Returns new <see cref="FilesystemDescriptor"/> instance with FileDescriptorLength set to new value.
        /// </summary>
        /// <param name="descriptor">Instance of <see cref="FilesystemDescriptor"/>.</param>
        /// <param name="fileDescriptorLength">New value for FileDescriptorLength.</param>
        /// <returns>New <see cref="FilesystemDescriptor"/> instance with FileDescriptorLength set to new value.</returns>
        public static FilesystemDescriptor WithFileDescriptorLength(this in FilesystemDescriptor descriptor, int fileDescriptorLength)
        {
            return new FilesystemDescriptor(
                descriptor.FilesDataLength,
                descriptor.FileDescriptorsCount,
                fileDescriptorLength);
        }
    }
}