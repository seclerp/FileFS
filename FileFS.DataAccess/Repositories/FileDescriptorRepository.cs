using System.Collections.Generic;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// File descriptor repository implementation.
    /// </summary>
    public class FileDescriptorRepository : IFileDescriptorRepository
    {
        private readonly IStorageConnection _connection;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ISerializer<FileDescriptor> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDescriptorRepository"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="serializer">File descriptor serializer instance.</param>
        public FileDescriptorRepository(
            IStorageConnection connection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ISerializer<FileDescriptor> serializer)
        {
            _connection = connection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _serializer = serializer;
        }

        /// <inheritdoc />
        public StorageItem<FileDescriptor> Read(int offset)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var length = filesystemDescriptor.FileDescriptorLength;
            var origin = SeekOrigin.End;
            var cursor = new Cursor(offset, origin);
            var data = _connection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            return new StorageItem<FileDescriptor>(in descriptor, in cursor);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll()
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(in filesystemDescriptor);

            var allDescriptors = new StorageItem<FileDescriptor>[filesystemDescriptor.FileDescriptorsCount];
            var index = 0;
            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var length = filesystemDescriptor.FileDescriptorLength;
                var origin = SeekOrigin.End;
                var data = _connection.PerformRead(new Cursor(offset, origin), length);
                var descriptor = _serializer.FromBuffer(data);
                var cursor = new Cursor(offset, origin);

                allDescriptors[index] = new StorageItem<FileDescriptor>(in descriptor, in cursor);
                index++;
            }

            return allDescriptors;
        }

        /// <inheritdoc />
        public void Write(StorageItem<FileDescriptor> item)
        {
            var data = _serializer.ToBuffer(item.Value);

            _connection.PerformWrite(item.Cursor, data);
        }

        /// <inheritdoc />
        public StorageItem<FileDescriptor> Find(string fileName)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(filesystemDescriptor);

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

        /// <inheritdoc />
        public bool Exists(string fileName)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(filesystemDescriptor);

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
            const SeekOrigin origin = SeekOrigin.End;

            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            var begin = new Cursor(startFromOffset, origin);
            var end = new Cursor(endOffset, origin);

            return new CursorRange(begin, end);
        }
    }
}