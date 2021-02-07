namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents file and its data.
    /// </summary>
    public readonly struct FileEntry
    {
        /// <summary>
        /// Name of the file.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// File data.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntry"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">File data.</param>
        public FileEntry(string fileName, byte[] data)
        {
            FileName = fileName;
            Data = data;
        }
    }
}