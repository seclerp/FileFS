using FileFS.Client;
using FileFS.Client.Abstractions;
using FileFS.Client.Configuration;
using FileFS.Client.Transactions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Serializers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileFS.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> type.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FileFS <see cref="IFileFsClient"/> implementation and all its dependencies to IoC container.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fileFsStoragePath">Path to a existing file that is used as FileFS storage.</param>
        /// <param name="options">Client options.</param>
        /// <returns>Configured instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFileFsClient(this IServiceCollection services, string fileFsStoragePath, FileFsClientOptions options)
        {
            FileFsClientOptionsValidator.Validate(options);

            services.AddSingleton<IStorageStreamProvider, StorageStreamProvider>(provider =>
                new StorageStreamProvider(fileFsStoragePath, provider.GetService<ILogger>()));

            services.AddSingleton<IStorageConnection, StorageConnection>(provider =>
                new StorageConnection(
                    provider.GetRequiredService<IStorageStreamProvider>(),
                    options.ByteBufferSize,
                    provider.GetRequiredService<ILogger>()));

            services.AddSingleton<ISerializer<FilesystemDescriptor>, FilesystemDescriptorSerializer>();
            services.AddSingleton<IFilesystemDescriptorAccessor, FilesystemDescriptorAccessor>();

            services.AddSingleton<ISerializer<FileDescriptor>, FileDescriptorSerializer>();
            services.AddSingleton<IFileDescriptorRepository, FileDescriptorRepository>();

            services.AddSingleton<IStorageOptimizer, StorageOptimizer>();
            services.AddSingleton<IFileAllocator, FileAllocator>();

            services.AddSingleton<IStorageInitializer, StorageInitializer>();

            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddSingleton<IExternalFileManager, ExternalFileManager>();

            if (options.EnableTransactions)
            {
                services.AddSingleton<ITransactionWrapper>(provider =>
                    new TransactionWrapper(fileFsStoragePath, provider.GetService<ILogger>()));
            }
            else
            {
                services.AddSingleton<ITransactionWrapper>(provider => new NullTransactionWrapper());
            }

            services.AddSingleton<IFileFsClient, FileFsClient>();

            return services;
        }

        /// <summary>
        /// Adds FileFS <see cref="IFileFsClient"/> implementation with default options and all its dependencies to IoC container.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fileFsStoragePath">Path to a existing file that is used as FileFS storage.</param>
        /// <returns>Configured instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFileFsClient(this IServiceCollection services, string fileFsStoragePath)
        {
            return services.AddFileFsClient(fileFsStoragePath, new FileFsClientOptions());
        }
    }
}