namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction responsible for initialization of new FileFS storage.
    /// </summary>
    public interface IStorageInitializer
    {
        /// <summary>
        /// Performs initialization of new FileFS storage.
        /// </summary>
        /// <param name="fileSize">Size that will be used as reserved for new FileFS storage.</param>
        /// <param name="fileNameLength">Maximum length for filenames.</param>
        void Initialize(int fileSize, int fileNameLength);
    }
}