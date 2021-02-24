using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using Serilog;

namespace FileFS.DataAccess.Allocation
{
    /// <summary>
    /// Implementation of storage extender.
    /// </summary>
    public class StorageExtender : IStorageExtender
    {
        private readonly IStorageConnection _connection;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageExtender"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor access instance.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageExtender(
            IStorageConnection connection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ILogger logger)
        {
            _connection = connection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Extend(long newSize)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var currentSize = _connection.GetSize();
            if (newSize <= currentSize)
            {
                throw new OperationIsInvalid(
                    $"New storage size should be greater that current size, current size {currentSize}, new size {newSize}");
            }

            _logger.Information($"Start storage resize process with new storage size {newSize}");

            _connection.SetSize(newSize);

            // Filesystem descriptor will be written at new position at the new end of storage
            var updatedFilesystemDescriptor =
                _filesystemDescriptorAccessor.Update(
                    _ => filesystemDescriptor.FilesDataLength,
                    _ => filesystemDescriptor.EntryDescriptorsCount,
                    _ => filesystemDescriptor.EntryDescriptorLength);

            CopyDescriptors(updatedFilesystemDescriptor, (int)currentSize, (int)newSize);

            _logger.Information($"Finish storage resize process with new storage size {newSize}");
        }

        private void CopyDescriptors(in FilesystemDescriptor filesystemDescriptor, int oldStorageSize, int newStorageSize)
        {
            var entriesSource = oldStorageSize - FilesystemDescriptor.BytesTotal -
                               (filesystemDescriptor.EntryDescriptorLength *
                                filesystemDescriptor.EntryDescriptorsCount);

            var entriesDestination = newStorageSize - FilesystemDescriptor.BytesTotal -
                                     (filesystemDescriptor.EntryDescriptorLength *
                                      filesystemDescriptor.EntryDescriptorsCount);

            var sourceCursor = new Cursor(entriesSource, SeekOrigin.Begin);
            var destinationCursor = new Cursor(entriesDestination, SeekOrigin.Begin);

            _connection.PerformCopy(sourceCursor, destinationCursor, filesystemDescriptor.EntryDescriptorLength * filesystemDescriptor.EntryDescriptorsCount);
        }
    }
}