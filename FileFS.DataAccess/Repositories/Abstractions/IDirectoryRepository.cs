using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstraction for directory repository.
    /// </summary>
    public interface IDirectoryRepository : IEntryRepository
    {
        /// <summary>
        /// Returns directory by given path.
        /// </summary>
        /// <param name="fullPath">Full path to a directory.</param>
        /// <returns>Object that represents directory.</returns>
        DirectoryEntry Find(string fullPath);

        /// <summary>
        /// Creates new directory in FileFS storage.
        /// </summary>
        /// <param name="fullPath">Full path to a newly created directory.</param>
        void Create(string fullPath);
    }
}