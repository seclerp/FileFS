using System.Collections.Generic;
using System.IO;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    public class FileDescriptorRepository : IFileDescriptorRepository
    {
        private readonly DataAccess.Abstractions.IStorageConnection _storageConnection;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly ISerializer<FileDescriptor> _serializer;

        public FileDescriptorRepository(
            DataAccess.Abstractions.IStorageConnection storageConnection,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            ISerializer<FileDescriptor> serializer)
        {
            _storageConnection = storageConnection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _serializer = serializer;
        }

        public StorageItem<FileDescriptor> Read(int offset)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var length = filesystemDescriptor.FileDescriptorLength;
            var origin = SeekOrigin.End;
            var cursor = new Cursor(offset, origin);
            var data = _storageConnection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            return new StorageItem<FileDescriptor>(ref descriptor, ref cursor);
        }

        public IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll()
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();

            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            var allDescriptors = new StorageItem<FileDescriptor>[filesystemDescriptor.FileDescriptorsCount];
            var index = 0;
            for (var offset = startFromOffset; offset >= endOffset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var length = filesystemDescriptor.FileDescriptorLength;
                var origin = SeekOrigin.End;
                var data = _storageConnection.PerformRead(new Cursor(offset, origin), length);
                var descriptor = _serializer.FromBuffer(data);
                var cursor = new Cursor(offset, origin);

                allDescriptors[index] = new StorageItem<FileDescriptor>(ref descriptor, ref cursor);
                index++;
            }

            return allDescriptors;
        }

        public void Write(StorageItem<FileDescriptor> item)
        {
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(item.Value);

            _storageConnection.PerformWrite(item.Cursor, data);
        }
    }
}