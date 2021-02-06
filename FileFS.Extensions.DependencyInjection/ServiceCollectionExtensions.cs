using FileFS.Client;
using FileFS.Client.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Serializers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileFS.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileFsClient(this IServiceCollection services, string fileFsPath)
        {
            services.AddSingleton<IStorageConnection, StorageConnection>(provider =>
                new StorageConnection(fileFsPath, provider.GetService<ILogger>()));

            services.AddSingleton<ISerializer<FilesystemDescriptor>, FilesystemDescriptorSerializer>();
            services.AddSingleton<IFilesystemDescriptorAccessor, FilesystemDescriptorAccessor>();

            services.AddSingleton<ISerializer<FileDescriptor>, FileDescriptorSerializer>();
            services.AddSingleton<IFileDescriptorRepository, FileDescriptorRepository>();

            services.AddSingleton<IStorageOptimizer, StorageOptimizer>();
            services.AddSingleton<IFileAllocator, FileAllocator>();

            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddSingleton<IExternalFileManager, ExternalFileManager>();
            services.AddSingleton<IFileFsClient, FileFsClient>();

            return services;
        }
    }
}