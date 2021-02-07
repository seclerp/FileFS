using FileFS.DataAccess.Entities.Abstractions;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents file and its byte array data representation.
    /// </summary>
    public readonly struct FileEntry : IFileEntry
    {
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

        /// <inheritdoc />
        public string FileName { get; }

        /// <inheritdoc />
        public int DataLength => Data.Length;
    }
}