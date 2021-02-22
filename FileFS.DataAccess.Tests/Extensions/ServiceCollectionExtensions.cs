using System;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
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

            services.InjectCommonServices();

            return services;
        }

        /// <summary>
        /// Adds FileFS data access layer dependencies with in memory stream provider.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <param name="storageFileName">Name of FileFS storage file.</param>
        /// <returns>Configured <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddFileFsDataAccessInMemory(this IServiceCollection services, string storageFileName)
        {
            services.AddSingleton<IStorageStreamProvider>(provider =>
                new StorageStreamProvider(storageFileName, provider.GetRequiredService<ILogger>()));

            services.InjectCommonServices();

            return services;
        }

        /// <summary>
        /// Initializes FileFS storage, using <see cref="IStorageInitializer"/> given from DI container.
        /// </summary>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="storageSize">Size of a storage in bytes.</param>
        /// <param name="fileNameLength">Maximum length of filename in bytes.</param>
        public static void InitializeStorage(this IServiceProvider serviceProvider, int storageSize, int fileNameLength)
        {
            var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();
            storageInitializer.Initialize(storageSize, fileNameLength);
        }

        private static IServiceCollection InjectCommonServices(this IServiceCollection services)
        {
            services.AddSingleton<IStorageConnection, StorageConnection>(provider =>
                new StorageConnection(
                    provider.GetRequiredService<IStorageStreamProvider>(),
                    4096,
                    provider.GetRequiredService<ILogger>()));

            services.AddSingleton<ISerializer<FilesystemDescriptor>, FilesystemDescriptorSerializer>();
            services.AddSingleton<IFilesystemDescriptorAccessor, FilesystemDescriptorAccessor>();

            services.AddSingleton<ISerializer<EntryDescriptor>, EntryDescriptorSerializer>();
            services.AddSingleton<IEntryDescriptorRepository, EntryDescriptorRepository>();

            services.AddSingleton<IStorageOptimizer, StorageOptimizer>();
            services.AddSingleton<IStorageExtender, StorageExtender>();
            services.AddSingleton<IFileAllocator, FileAllocator>();

            services.AddSingleton<IStorageInitializer, StorageInitializer>();

            services.AddSingleton<IEntryRepository, EntryRepository>();
            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddSingleton<IDirectoryRepository, DirectoryRepository>();

            return services;
        }
    }
}