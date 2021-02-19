namespace FileFS.DataAccess.Entities.Abstractions
{
    public interface IFileEntry : IEntry
    {
        /// <summary>
        /// Gets length of the data in bytes.
        /// </summary>
        public int DataLength { get; }
    }
}