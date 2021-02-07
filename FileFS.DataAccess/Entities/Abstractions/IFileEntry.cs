namespace FileFS.DataAccess.Entities.Abstractions
{
    /// <summary>
    /// Abstraction for every file entry implementation.
    /// </summary>
    public interface IFileEntry
    {
        /// <summary>
        /// Gets name of the file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets length of the data in bytes.
        /// </summary>
        public int DataLength { get; }
    }
}