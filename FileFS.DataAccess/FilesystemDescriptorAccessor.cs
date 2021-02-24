using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// Filesystem descriptor access implementation.
    /// </summary>
    public class FilesystemDescriptorAccessor : IFilesystemDescriptorAccessor
    {
        private readonly IStorageConnection _connection;
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly ILogger _logger;

        private readonly object _locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemDescriptorAccessor"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="serializer">Filesystem descriptor Serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public FilesystemDescriptorAccessor(
            IStorageConnection connection,
            ISerializer<FilesystemDescriptor> serializer,
            ILogger logger)
        {
            _connection = connection;
            _serializer = serializer;
            _logger = logger;
            _locker = new object();
        }

        /// <inheritdoc />
        public FilesystemDescriptor Value => Read();

        /// <inheritdoc />
        public FilesystemDescriptor Update(
            Func<int, int> filesDataLengthUpdater = null,
            Func<int, int> entryDescriptorsCountUpdater = null,
            Func<int, int> entryDescriptorLengthUpdater = null)
        {
            lock (_locker)
            {
                var descriptor = Read();
                var newFilesDataLength = filesDataLengthUpdater?.Invoke(descriptor.FilesDataLength) ?? descriptor.FilesDataLength;
                var newEntryDescriptorsCount = entryDescriptorsCountUpdater?.Invoke(descriptor.EntryDescriptorsCount) ?? descriptor.EntryDescriptorsCount;
                var newEntryDescriptorLength = entryDescriptorLengthUpdater?.Invoke(descriptor.EntryDescriptorLength) ?? descriptor.EntryDescriptorLength;

                var updatedDescriptor = new FilesystemDescriptor(newFilesDataLength, newEntryDescriptorsCount, newEntryDescriptorLength);

                UpdateInternal(updatedDescriptor);

                return updatedDescriptor;
            }
        }

        private void UpdateInternal(FilesystemDescriptor descriptor)
        {
            _logger.Information("Trying to update filesystem descriptor");

            var offset = -FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _serializer.ToBytes(descriptor);

            _connection.PerformWrite(new Cursor(offset, origin), data);

            _logger.Information("Filesystem descriptor updated");
        }

        private FilesystemDescriptor Read()
        {
            lock (_locker)
            {
                _logger.Information("Trying to retrieve filesystem descriptor");

                const SeekOrigin origin = SeekOrigin.End;

                var offset = -FilesystemDescriptor.BytesTotal;
                var length = FilesystemDescriptor.BytesTotal;
                var data = _connection.PerformRead(new Cursor(offset, origin), length);
                var descriptor = _serializer.FromBytes(data);

                _logger.Information("Filesystem descriptor retrieved");

                return descriptor;
            }
        }
    }
}