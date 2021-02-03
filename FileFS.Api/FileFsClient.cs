using FileFS.Api.Abstractions;
using FileFs.DataAccess;
using FileFs.DataAccess.Repositories;
using FileFs.DataAccess.Serializers;
using FileFS.Managers;

namespace FileFS.Api
{
    public class FileFsClient : IFileFsClient
    {
        private readonly FileFsManager _manager;

        public FileFsClient(string fileFsPath)
        {
            var connection = new FileFsConnection(fileFsPath);

            var filesystemSerializer = new FilesystemDescriptorSerializer();
            var filesystemRepository = new FilesystemDescriptorRepository(connection, filesystemSerializer);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemRepository);
            var fileDescriptorRepository = new FileDescriptorRepository(connection, filesystemRepository, fileDescriptorSerializer);

            var fileDataRepository = new FileDataRepository(connection);

            var allocator = new FileAllocator(connection, filesystemRepository);

            _manager = new FileFsManager(allocator, fileDataRepository, filesystemRepository, fileDescriptorRepository);
        }

        public void Create(string fileName, byte[] content)
        {
            _manager.Create(fileName, content);
        }

        public void Update(string fileName, byte[] newContent)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string fileName)
        {
            _manager.Delete(fileName);
        }

        public byte[] Read(string fileName)
        {
            return _manager.Read(fileName);
        }

        public bool Exists(string fileName)
        {
            return _manager.Exists(fileName);
        }

        public void Rename(string oldName, string newName)
        {
            _manager.Rename(oldName, newName);
        }
    }
}