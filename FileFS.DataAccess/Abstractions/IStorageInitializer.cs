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
        /// <param name="fileFsStoragePath">Path to a new file that will be used as FileFS storage.</param>
        /// <param name="fileSize">Size that will be used as reserved for new FileFS storage.</param>
        /// <param name="fileNameLength">Maximum length for filenames.</param>
        void Initialize(string fileFsStoragePath, int fileSize, int fileNameLength);
    }
}