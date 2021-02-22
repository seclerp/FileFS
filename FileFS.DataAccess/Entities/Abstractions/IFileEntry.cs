namespace FileFS.DataAccess.Entities.Abstractions
{
    /// <summary>
    /// Abstraction for every file entry in FileFS storage.
    /// </summary>
    public interface IFileEntry : IEntry
    {
        /// <summary>
        /// Gets length of the data in bytes.
        /// </summary>
        public int DataLength { get; }
    }
}