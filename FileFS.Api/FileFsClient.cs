using System.Collections.Generic;
using FileFS.Api.Abstractions;
using FileFs.DataAccess;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories;
using FileFs.DataAccess.Serializers;
using FileFS.Managers;
using Serilog;

namespace FileFS.Api
{
    public class FileFsClient : IFileFsClient
    {
        private readonly FileFsManager _manager;

        public FileFsClient(string fileFsPath, ILogger logger)
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

            _manager = new FileFsManager(fileRepository, externalFileManager, optimizer);
        }

        public void Create(string fileName, byte[] content)
        {
            _manager.Create(fileName, content);
        }

        public void Update(string fileName, byte[] newContent)
        {
            _manager.Update(fileName, newContent);
        }

        public void Delete(string fileName)
        {
            _manager.Delete(fileName);
        }

        public void Import(string externalPath, string fileName)
        {
            _manager.Import(externalPath, fileName);
        }

        public void Export(string fileName, string externalPath)
        {
            _manager.Export(fileName, externalPath);
        }

        public byte[] Read(string fileName)
        {
            return _manager.ReadContent(fileName);
        }

        public bool Exists(string fileName)
        {
            return _manager.Exists(fileName);
        }

        public void Rename(string oldName, string newName)
        {
            _manager.Rename(oldName, newName);
        }

        public IReadOnlyCollection<FileEntryInfo> List()
        {
            return _manager.List();
        }

        public void ForceOptimize()
        {
            _manager.ForceOptimize();
        }
    }
}