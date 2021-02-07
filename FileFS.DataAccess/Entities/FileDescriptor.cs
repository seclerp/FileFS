namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents file descriptor in FileFS storage.
    /// </summary>
    public readonly struct FileDescriptor
    {
        /// <summary>
        /// Amount of bytes used to store file descriptor data, except filename.
        /// </summary>
        public static readonly int BytesWithoutFilename = 28;

        /// <summary>
        /// Actual length of the filename.
        /// </summary>
        public readonly int FileNameLength;

        /// <summary>
        /// Name of the file.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Unix timestamp that represents time when file was created.
        /// </summary>
        public readonly long CreatedOn;

        /// <summary>
        /// Unix timestamp that represents time when file was changed last time.
        /// </summary>
        public readonly long UpdatedOn;

        /// <summary>
        /// Offset of the file data in memory.
        /// </summary>
        public readonly int DataOffset;

        /// <summary>
        /// Length of the file data.
        /// </summary>
        public readonly int DataLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDescriptor"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="createdOn">Unix timestamp that represents time when file was created.</param>
        /// <param name="updatedOn">Unix timestamp that represents time when file was changed last time.</param>
        /// <param name="dataOffset">Offset of the file data in memory.</param>
        /// <param name="dataLength">Length of the file data.</param>
        public FileDescriptor(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            FileNameLength = fileName.Length;
            FileName = fileName;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }
    }
}