using System.IO;

namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction that provider stream for <see cref="IStorageConnection"/>.
    /// </summary>
    public interface IStorageStreamProvider
    {
        /// <summary>
        /// Opens stream to work with FileFS storage.
        /// </summary>
        /// <returns>Ready to use stream to work with FileFS storage.</returns>
        Stream OpenStream();
    }
}