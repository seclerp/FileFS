using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Allocation
{
    /// <summary>
    /// Storage optimizer implementation.
    /// </summary>
    public class StorageOptimizer : IStorageOptimizer
    {
        private readonly IStorageConnection _connection;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageOptimizer"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="entryDescriptorRepository">File descriptor repository instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageOptimizer(
            IStorageConnection connection,
            IEntryDescriptorRepository entryDescriptorRepository,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ILogger logger)
        {
            _connection = connection;
            _entryDescriptorRepository = entryDescriptorRepository;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public int Optimize()
        {
            _logger.Information("Start optimization process");

            var dataItemsMoved = 0;
            var initialDataSize = _filesystemDescriptorAccessor.Value.FilesDataLength;

            // 1. Get all descriptors
            var descriptors = _entryDescriptorRepository.ReadAll();

            _logger.Information($"There is {descriptors.Count} descriptors found");

            // 2. Sort by offset, filter zero-sized data because it is not necessary to move empty data
            var orderedDescriptors = descriptors
                .Where(descriptor => descriptor.Value.DataLength > 0)
                .OrderBy(descriptor => descriptor.Value.DataOffset)
                .ToArray();

            // 3a. Move first
            if (orderedDescriptors.Length > 0 && orderedDescriptors[0].Value.DataOffset != 0)
            {
                ProcessGap(orderedDescriptors, 0, 0);
                dataItemsMoved++;
            }

            // 3b. Iterate over all descriptors to find gaps
            for (var i = 0; i < orderedDescriptors.Length - 1; i++)
            {
                var current = orderedDescriptors[i];
                var next = orderedDescriptors[i + 1];
                var currentEnd = current.Value.DataOffset + current.Value.DataLength;
                var nextStart = next.Value.DataOffset;

                // 4. Found a gap, write data of second right after first
                if (nextStart - currentEnd > 0)
                {
                    ProcessGap(orderedDescriptors, i + 1, currentEnd);
                    dataItemsMoved++;
                }
            }

            // 4. Update new size of file data in descriptor.
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var updatedFilesystemDescriptor =
                filesystemDescriptor
                    .WithFileDataLength(orderedDescriptors.Select(descriptor => descriptor.Value.DataLength).Sum());

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);

            var bytesOptimized = initialDataSize - updatedFilesystemDescriptor.FilesDataLength;

            _logger.Information($"Optimization process completed, {dataItemsMoved} items moved, {bytesOptimized} bytes optimized");

            return bytesOptimized;
        }

        private void ProcessGap(IList<StorageItem<EntryDescriptor>> orderedDescriptors, int descriptorIndex, int gapOffset)
        {
            var movingDataDescriptorItem = orderedDescriptors[descriptorIndex];

            _logger.Information($"Found gap of size {movingDataDescriptorItem.Value.DataOffset - 1} on offset {gapOffset}");

            var newStorageItem = CopyFile(movingDataDescriptorItem.Value, movingDataDescriptorItem.Cursor, gapOffset);
            orderedDescriptors[descriptorIndex] = newStorageItem;
        }

        private StorageItem<EntryDescriptor> CopyFile(EntryDescriptor entryDescriptor, Cursor cursor, int destinationOffset)
        {
            var newDescriptor = new EntryDescriptor(
                entryDescriptor.Id,
                entryDescriptor.ParentId,
                entryDescriptor.Name,
                entryDescriptor.Type,
                entryDescriptor.CreatedOn,
                entryDescriptor.UpdatedOn,
                destinationOffset,
                entryDescriptor.DataLength);
            var newStorageItem = new StorageItem<EntryDescriptor>(in newDescriptor, in cursor);

            _logger.Information($"Moving {entryDescriptor.DataLength} bytes of data from {entryDescriptor.DataOffset} to {destinationOffset}");

            PerformCopy(entryDescriptor.DataOffset, destinationOffset, entryDescriptor.DataLength);
            _entryDescriptorRepository.Write(newStorageItem);

            _logger.Information($"{entryDescriptor.DataLength} bytes of data moved to offset {destinationOffset}");

            return newStorageItem;
        }

        private void PerformCopy(int sourceOffset, int destinationOffset, int length)
        {
            const SeekOrigin origin = SeekOrigin.Begin;

            _connection.PerformCopy(new Cursor(sourceOffset, origin), new Cursor(destinationOffset, origin), length);
        }
    }
}