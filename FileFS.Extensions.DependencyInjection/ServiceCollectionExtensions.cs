using FileFS.Client;
using FileFS.Client.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Memory;
using FileFS.DataAccess.Memory.Abstractions;
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
        /// <returns>Configured instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFileFsClient(this IServiceCollection services, string fileFsStoragePath)
        {
            services.AddSingleton<IStorageStreamProvider, StorageStreamProvider>(provider =>
                new StorageStreamProvider(fileFsStoragePath, provider.GetService<ILogger>()));

            services.AddSingleton<IStorageConnection, StorageConnection>();

            services.AddSingleton<ISerializer<FilesystemDescriptor>, FilesystemDescriptorSerializer>();
            services.AddSingleton<IFilesystemDescriptorAccessor, FilesystemDescriptorAccessor>();

            services.AddSingleton<ISerializer<FileDescriptor>, FileDescriptorSerializer>();
            services.AddSingleton<IFileDescriptorRepository, FileDescriptorRepository>();

            services.AddSingleton<IStorageOptimizer, StorageOptimizer>();
            services.AddSingleton<IFileAllocator, FileAllocator>();

            services.AddSingleton<IStorageInitializer, StorageInitializer>();

            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddSingleton<IExternalFileManager, ExternalFileManager>();
            services.AddSingleton<ITransactionWrapper>(provider =>
                new TransactionWrapper(fileFsStoragePath, provider.GetService<ILogger>()));

            services.AddSingleton<IFileFsClient, FileFsClient>();

            return services;
        }
    }
}