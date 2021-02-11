using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Memory;
using FileFS.DataAccess.Memory.Abstractions;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Serializers.Abstractions;
using FileFS.Tests.Shared.Factories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileFS.DataAccess.Tests.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> type.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FileFS data access layer dependencies with in memory stream provider.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <param name="storageBuffer">Storage buffer.</param>
        /// <returns>Configured <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddFileFsDataAccessInMemory(this IServiceCollection services, byte[] storageBuffer)
        {
            services.AddSingleton<IStorageStreamProvider>(provider =>
                StorageStreamProviderMockFactory.Create(storageBuffer));

            services.AddSingleton<IStorageConnection, StorageConnection>(provider =>
                new StorageConnection(
                    provider.GetRequiredService<IStorageStreamProvider>(),
                    4096,
                    provider.GetRequiredService<ILogger>()));

            services.AddSingleton<ISerializer<FilesystemDescriptor>, FilesystemDescriptorSerializer>();
            services.AddSingleton<IFilesystemDescriptorAccessor, FilesystemDescriptorAccessor>();

            services.AddSingleton<ISerializer<FileDescriptor>, FileDescriptorSerializer>();
            services.AddSingleton<IFileDescriptorRepository, FileDescriptorRepository>();

            services.AddSingleton<IStorageOptimizer, StorageOptimizer>();
            services.AddSingleton<IFileAllocator, FileAllocator>();

            services.AddSingleton<IFileRepository, FileRepository>();

            services.AddSingleton<IStorageInitializer, StorageInitializer>();

            return services;
        }
    }
}