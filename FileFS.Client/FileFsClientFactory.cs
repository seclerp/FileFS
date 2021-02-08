using FileFS.Client.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Memory;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Serializers;
using Serilog;

namespace FileFS.Client
{
    /// <summary>
    /// Class that allows to create configured <see cref="IFileFsClient"/> instances.
    /// </summary>
    public static class FileFsClientFactory
    {
        /// <summary>
        /// Creates instance of <see cref="IFileFsClient"/>.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="logger">Logger instance.</param>
        /// <returns>Instance of <see cref="IFileFsClient"/>.</returns>
        public static IFileFsClient Create(string fileFsStoragePath, ILogger logger)
        {
            var storageStreamProvider = new StorageStreamProvider(fileFsStoragePath, logger);
            var connection = new StorageConnection(storageStreamProvider, logger);

            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var filesystemDescriptorAccessor = new FilesystemDescriptorAccessor(connection, filesystemDescriptorSerializer, logger);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var fileDescriptorRepository = new FileDescriptorRepository(connection, filesystemDescriptorAccessor, fileDescriptorSerializer, logger);

            var optimizer = new StorageOptimizer(connection, fileDescriptorRepository, logger);
            var allocator = new FileAllocator(connection, filesystemDescriptorAccessor, fileDescriptorRepository, optimizer, logger);

            var fileRepository = new FileRepository(connection, allocator, filesystemDescriptorAccessor, fileDescriptorRepository, logger);

            var externalFileManager = new ExternalFileManager(logger);

            var client = new FileFsClient(fileRepository, externalFileManager, optimizer);

            return client;
        }
    }
}