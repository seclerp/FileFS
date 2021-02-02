using FileFs.DataAccess;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories;
using FileFs.DataAccess.Serializers;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFS.Cli
{
    class Program
    {
        private static void CreateNew(ISerializer<FilesystemDescriptor> filesystemDescriptorSerializer, string fileName)
        {
            var manager = new FileFsInitializer(filesystemDescriptorSerializer);
            manager.Initialize(fileName, 10 * 1024 * 1024, 256, 1);
        }

        private static IFileFsConnection Open(string fileName)
        {
            var connection = new FileFsConnection(fileName);
            return connection;
        }

        static void Main(string[] args)
        {
            var fileName = "filefs";
            var filesystemSerializer = new FilesystemDescriptorSerializer();

            CreateNew(filesystemSerializer, fileName);
            var connection = Open(fileName);

            var filesystemRepository = new FilesystemDescriptorRepository(connection, filesystemSerializer);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemRepository);
            var fileDescriptorRepository = new FileDescriptorRepository(connection, filesystemRepository, fileDescriptorSerializer);

            var newDescriptor = new FileDescriptor("example", 123, 321);

            var filesystemDescriptor = filesystemRepository.Read();
            var fileDescriptorOffset = filesystemDescriptor.FileDescriptorLength;
            fileDescriptorRepository.Write(newDescriptor, -FilesystemDescriptor.BytesTotal - fileDescriptorOffset);

            var newDescriptorRetrieved = fileDescriptorRepository.Read(-FilesystemDescriptor.BytesTotal - fileDescriptorOffset);
        }
    }
}