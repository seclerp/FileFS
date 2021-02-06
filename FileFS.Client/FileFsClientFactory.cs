using FileFS.Client.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Serializers;
using Serilog;

namespace FileFS.Client
{
    public class FileFsClientFactory
    {
        public static IFileFsClient Create(string fileFsPath, ILogger logger)
        {
            var connection = new StorageConnection(fileFsPath, logger);

            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer();
            var filesystemDescriptorAccessor = new FilesystemDescriptorAccessor(connection, filesystemDescriptorSerializer);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemDescriptorAccessor);
            var fileDescriptorRepository = new FileDescriptorRepository(connection, filesystemDescriptorAccessor, fileDescriptorSerializer);

            var optimizer = new StorageOptimizer(connection, fileDescriptorRepository, logger);
            var allocator = new FileAllocator(connection, filesystemDescriptorAccessor, fileDescriptorRepository, optimizer, logger);

            var fileRepository = new FileRepository(connection, allocator, filesystemDescriptorAccessor, fileDescriptorRepository, logger);

            var externalFileManager = new ExternalFileManager(logger);

            var manager = new FileFsClient(fileRepository, externalFileManager, optimizer);

            return manager;
        }
    }
}