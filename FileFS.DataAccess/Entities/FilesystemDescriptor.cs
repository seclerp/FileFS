using System;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents filesystem descriptor in FileFS storage.
    /// </summary>
    public readonly struct FilesystemDescriptor
    {
        /// <summary>
        /// Amount of bytes used to store filesystem descriptor.
        /// </summary>
        public static readonly int BytesTotal = 12;

        /// <summary>
        /// Overall amount of allocated bytes used to store files data.
        /// This value also includes memory that potentially could be optimized.
        /// </summary>
        public readonly int FilesDataLength;

        /// <summary>
        /// Amount of file descriptors in storage.
        /// </summary>
        public readonly int FileDescriptorsCount;

        /// <summary>
        /// Overall amount of bytes used to store file descriptors data.
        /// </summary>
        public readonly int FileDescriptorLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemDescriptor"/> struct.
        /// </summary>
        /// <param name="filesDataLength">Overall amount of allocated bytes used to store files data.</param>
        /// <param name="fileDescriptorsCount">Amount of file descriptors in storage.</param>
        /// <param name="fileDescriptorLength">Overall amount of bytes used to store file descriptors data.</param>
        public FilesystemDescriptor(int filesDataLength, int fileDescriptorsCount, int fileDescriptorLength)
        {
            FilesDataLength = filesDataLength;
            FileDescriptorsCount = fileDescriptorsCount;
            FileDescriptorLength = fileDescriptorLength;
        }
    }
}