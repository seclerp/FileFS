using System.Collections.Generic;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;

namespace FileFS.DataAccess.Repositories
{
    public class FileDescriptorRepository : IFileDescriptorRepository
    {
        private readonly IStorageConnection _storageConnection;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ISerializer<FileDescriptor> _serializer;

        public FileDescriptorRepository(
            IStorageConnection storageConnection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ISerializer<FileDescriptor> serializer)
        {
            _storageConnection = storageConnection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _serializer = serializer;
        }

        public StorageItem<FileDescriptor> Read(int offset)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var length = filesystemDescriptor.FileDescriptorLength;
            var origin = SeekOrigin.End;
            var cursor = new Cursor(offset, origin);
            var data = _storageConnection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            return new StorageItem<FileDescriptor>(ref descriptor, ref cursor);
        }

        public IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll()
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;

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

        public StorageItem<FileDescriptor> Find(string fileName)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(in filesystemDescriptor);

            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var currentDescriptor = Read(offset);
                if (currentDescriptor.Value.FileName == fileName)
                {
                    return currentDescriptor;
                }
            }

            return default;
        }

        public bool Exists(string fileName)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(in filesystemDescriptor);

            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var currentDescriptor = Read(offset);
                if (currentDescriptor.Value.FileName == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        private CursorRange GetDescriptorsRange(in FilesystemDescriptor filesystemDescriptor)
        {
            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            var origin = SeekOrigin.End;
            var begin = new Cursor(startFromOffset, origin);
            var end = new Cursor(endOffset, origin);

            return new CursorRange(begin, end);
        }
    }
}