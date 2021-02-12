using FileFS.DataAccess;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Serializers;
using Serilog;

namespace FileFS.Client
{
    /// <summary>
    /// Class that allows to create configured <see cref="IStorageInitializer"/> instances.
    /// </summary>
    public class StorageInitializerFactory
    {
        /// <summary>
        /// Creates instance of <see cref="IStorageInitializer"/>.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="logger">Logger instance.</param>
        /// <returns>Instance of <see cref="IStorageInitializer"/>.</returns>
        public static IStorageInitializer Create(string fileFsStoragePath, ILogger logger)
        {
            var storageStreamProvider = new StorageStreamProvider(fileFsStoragePath, logger);
            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var storageInitializer =
                new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);

            return storageInitializer;
        }
    }
}