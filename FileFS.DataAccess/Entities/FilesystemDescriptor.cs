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
        public readonly int EntryDescriptorsCount;

        /// <summary>
        /// Overall amount of bytes used to store file descriptors data.
        /// </summary>
        public readonly int EntryDescriptorLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemDescriptor"/> struct.
        /// </summary>
        /// <param name="filesDataLength">Overall amount of allocated bytes used to store files data.</param>
        /// <param name="entryDescriptorsCount">Amount of file descriptors in storage.</param>
        /// <param name="entryDescriptorLength">Overall amount of bytes used to store file descriptors data.</param>
        public FilesystemDescriptor(int filesDataLength, int entryDescriptorsCount, int entryDescriptorLength)
        {
            FilesDataLength = filesDataLength;
            EntryDescriptorsCount = entryDescriptorsCount;
            EntryDescriptorLength = entryDescriptorLength;
        }
    }
}