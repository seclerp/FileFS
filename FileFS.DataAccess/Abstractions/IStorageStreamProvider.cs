using System.IO;
using FileFS.DataAccess.Exceptions;

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
        /// <param name="checkExistence">If true, exception of type <see cref="StorageNotFoundException"/> will be thrown if stream target not exists.</param>
        /// <returns>Ready to use stream to work with FileFS storage.</returns>
        Stream OpenStream(bool checkExistence = true);
    }
}