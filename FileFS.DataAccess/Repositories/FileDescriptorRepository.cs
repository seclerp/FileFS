using System.Collections.Generic;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

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
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDescriptorRepository"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="serializer">File descriptor serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public FileDescriptorRepository(
            IStorageConnection connection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ISerializer<FileDescriptor> serializer,
            ILogger logger)
        {
            _connection = connection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public StorageItem<FileDescriptor> Read(int offset)
        {
            _logger.Information("Start file descriptor data reading process");

            const SeekOrigin origin = SeekOrigin.End;

            _logger.Information("Retrieving info about file descriptor length");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var length = filesystemDescriptor.FileDescriptorLength;

            var cursor = new Cursor(offset, origin);

            _logger.Information("Reading file descriptor data");

            var data = _connection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBytes(data);

            _logger.Information("Done reading file descriptor data");

            return new StorageItem<FileDescriptor>(in descriptor, in cursor);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll()
        {
            _logger.Information("Start reading all file descriptors data");

            _logger.Information("Retrieving info about file descriptors from filesystem descriptor");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(in filesystemDescriptor);

            _logger.Information("Reading all file descriptors data");

            var allDescriptors = new StorageItem<FileDescriptor>[filesystemDescriptor.FileDescriptorsCount];
            var index = 0;
            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var cursor = new Cursor(offset, SeekOrigin.End);

                var length = filesystemDescriptor.FileDescriptorLength;
                var data = _connection.PerformRead(cursor, length);
                var descriptor = _serializer.FromBytes(data);

                allDescriptors[index] = new StorageItem<FileDescriptor>(descriptor, cursor);
                index++;
            }

            _logger.Information("Done reading all file descriptors data");

            return allDescriptors;
        }

        /// <inheritdoc />
        public void Write(StorageItem<FileDescriptor> item)
        {
            _logger.Information("Start file descriptor data writing process");

            var data = _serializer.ToBytes(item.Value);

            _connection.PerformWrite(item.Cursor, data);

            _logger.Information("Done writing file descriptor data");
        }

        /// <inheritdoc />
        public StorageItem<FileDescriptor> Find(string fileName)
        {
            _logger.Information("Start file descriptor search process");

            _logger.Information("Retrieving info about file descriptors from filesystem descriptor");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(filesystemDescriptor);

            _logger.Information("Searching for specific file descriptor");

            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var cursor = new Cursor(offset, SeekOrigin.End);
                var data = _connection.PerformRead(cursor, filesystemDescriptor.FileDescriptorLength);
                var currentDescriptor = _serializer.FromBytes(data);

                if (currentDescriptor.FileName == fileName)
                {
                    _logger.Information("Specific descriptor found");

                    return new StorageItem<FileDescriptor>(currentDescriptor, cursor);
                }
            }

            _logger.Information("Specific descriptor not found");

            return default;
        }

        /// <inheritdoc />
        public bool Exists(string fileName)
        {
            _logger.Information("Start file descriptor existence checking process");

            _logger.Information("Retrieving info about file descriptors from filesystem descriptor");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(filesystemDescriptor);

            _logger.Information("Searching for specific file descriptor");

            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var cursor = new Cursor(offset, SeekOrigin.End);
                var data = _connection.PerformRead(cursor, filesystemDescriptor.FileDescriptorLength);
                var currentDescriptor = _serializer.FromBytes(data);

                if (currentDescriptor.FileName == fileName)
                {
                    _logger.Information("Specific descriptor found");

                    return true;
                }
            }

            _logger.Information("Specific descriptor not found");

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