using System.Diagnostics.CodeAnalysis;
using FileFS.Client.Abstractions;
using FileFS.Client.Configuration;
using FileFS.Client.Transactions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Allocation;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Serializers;
using Serilog;

namespace FileFS.Client
{
    /// <summary>
    /// Class that allows to create configured <see cref="IFileFsClient"/> instances.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class FileFsClientFactory
    {
        /// <summary>
        /// Creates instance of <see cref="IFileFsClient"/>.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="options">Options for FileFS client.</param>
        /// <param name="logger">Logger instance.</param>
        /// <returns>Instance of <see cref="IFileFsClient"/>.</returns>
        public static IFileFsClient Create(string fileFsStoragePath, FileFsClientOptions options, ILogger logger)
        {
            ITransactionWrapper CreateTransactionWrapper(bool enableTransactions) =>
                enableTransactions
                    ? (ITransactionWrapper)new TransactionWrapper(fileFsStoragePath, logger)
                    : (ITransactionWrapper)new NullTransactionWrapper();

            FileFsClientOptionsValidator.Validate(options);

            var storageStreamProvider = new StorageStreamProvider(fileFsStoragePath, logger);
            var connection = new StorageConnection(storageStreamProvider, options.ByteBufferSize, logger);

            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var filesystemDescriptorAccessor = new FilesystemDescriptorAccessor(connection, filesystemDescriptorSerializer, logger);

            var entryDescriptorSerializer = new EntryDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var entryDescriptorRepository = new EntryDescriptorRepository(connection, filesystemDescriptorAccessor, entryDescriptorSerializer, logger);

            var optimizer = new StorageOptimizer(connection, entryDescriptorRepository, filesystemDescriptorAccessor, logger);
            var extender = new StorageExtender(connection, filesystemDescriptorAccessor, logger);
            var operationLocker = new StorageOperationLocker();
            var allocator = new FileAllocator(connection, filesystemDescriptorAccessor, entryDescriptorRepository, optimizer, extender, operationLocker, logger);

            var entryRepository = new EntryRepository(filesystemDescriptorAccessor, entryDescriptorRepository, logger);
            var fileRepository = new FileRepository(connection, allocator, filesystemDescriptorAccessor, entryDescriptorRepository, logger);
            var directoryRepository = new DirectoryRepository(filesystemDescriptorAccessor, entryDescriptorRepository, logger);

            var externalFileManager = new ExternalFileManager(logger);
            var transactionWrapper = CreateTransactionWrapper(options.EnableTransactions);

            var client = new FileFsClient(fileRepository, directoryRepository, entryRepository, externalFileManager, optimizer, transactionWrapper);

            return client;
        }

        /// <summary>
        /// Creates instance of <see cref="IFileFsClient"/> with default options.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="logger">Logger instance.</param>
        /// <returns>Instance of <see cref="IFileFsClient"/>.</returns>
        public static IFileFsClient Create(string fileFsStoragePath, ILogger logger)
        {
            return Create(fileFsStoragePath, new FileFsClientOptions(), logger);
        }
    }
}