using System;
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
    /// Linear file allocator implementation.
    /// </summary>
    public class FileAllocator : IFileAllocator
    {
        private readonly IStorageConnection _connection;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly IStorageOptimizer _optimizer;
        private readonly IStorageExtender _storageExtender;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAllocator"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="entryDescriptorRepository">File descriptor repository instance.</param>
        /// <param name="optimizer">Storage optimizer instance.</param>
        /// <param name="storageExtender">Storage extender instance.</param>
        /// <param name="logger">Logger instance.</param>
        public FileAllocator(
            IStorageConnection connection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IEntryDescriptorRepository entryDescriptorRepository,
            IStorageOptimizer optimizer,
            IStorageExtender storageExtender,
            ILogger logger)
        {
            _connection = connection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _entryDescriptorRepository = entryDescriptorRepository;
            _optimizer = optimizer;
            _storageExtender = storageExtender;
            _logger = logger;
        }

        /// <inheritdoc />
        public Cursor AllocateFile(int dataSize)
        {
            _logger.Information($"Start memory allocation flow for {dataSize} bytes");

            // 0. If file data size is 0 - always return zero-pointed cursor
            if (dataSize is 0)
            {
                _logger.Information($"Skipping allocation due to 0-sized data");

                return new Cursor(0, SeekOrigin.Begin);
            }

            // 1. Try find existing gap of given size
            if (TryFindGap(dataSize, out var cursor))
            {
                _logger.Information($"Done performing allocation of {dataSize} bytes");

                return cursor;
            }

            // 2. No gaps with given size exists - try to allocate known empty space
            if (!CouldAllocate(dataSize))
            {
                _logger.Information($"Failed to allocate new space with size of {dataSize} bytes, starting optimizer");

                // First try to optimize space
                _optimizer.Optimize();

                // Recheck
                if (!CouldAllocate(dataSize))
                {
                    var totalAllocatedSpace = GetTotalAllocatedSpace();
                    var currentReservedSize = _connection.GetSize();
                    var timesResize = (int)Math.Ceiling(Math.Log((totalAllocatedSpace + dataSize) / (double)currentReservedSize, 2));

                    _storageExtender.Extend(currentReservedSize * (long)Math.Pow(2, timesResize));
                }

                return PerformAllocate(dataSize);
            }

            return PerformAllocate(dataSize);
        }

        private bool CouldAllocate(int dataSize)
        {
            _logger.Information($"Checking possibility to allocate new {dataSize} bytes");

            var overallSpace = _connection.GetSize();
            var totalAllocatedSpace = GetTotalAllocatedSpace();
            var remainingSpace = overallSpace - totalAllocatedSpace;

            var couldAllocate = remainingSpace >= dataSize;

            _logger.Information($"Could allocate decision: {couldAllocate}");

            return couldAllocate;
        }

        private int GetTotalAllocatedSpace()
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var specialSpace = FilesystemDescriptor.BytesTotal +
                               (filesystemDescriptor.EntryDescriptorsCount * filesystemDescriptor.EntryDescriptorLength);

            var dataSpace = filesystemDescriptor.FilesDataLength;

            return specialSpace + dataSpace;
        }

        private Cursor PerformAllocate(int dataSize)
        {
            _logger.Information($"Performing allocation for newly acquired space of {dataSize} bytes");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var newDataOffset = filesystemDescriptor.FilesDataLength;
            var newDataCursor = new Cursor(newDataOffset, SeekOrigin.Begin);

            _logger.Information($"Space allocated at offset {newDataOffset}");
            _logger.Information("Updating filesystem descriptor");

            var updatedFilesystemDescriptor = filesystemDescriptor
                .WithFileDataLength(filesystemDescriptor.FilesDataLength + dataSize);

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);

            _logger.Information("filesystem descriptor updated");
            _logger.Information($"Done performing allocation of {dataSize} bytes");

            return newDataCursor;
        }

        private bool TryFindGap(int size, out Cursor cursor)
        {
            _logger.Information($"Trying to find existing gap of {size} bytes");

            cursor = default;

            // 1. Get all descriptors
            var descriptors = _entryDescriptorRepository.ReadAll();

            // 2. Sort by offset
            var orderedDescriptors = descriptors
                .Where(descriptor => descriptor.Value.DataLength > 0)
                .OrderBy(descriptor => descriptor.Value.DataOffset).ToArray();

            // 3. Check first gap
            if (orderedDescriptors.Length > 0 && orderedDescriptors[0].Value.DataOffset >= size)
            {
                // Start of storage is our gap
                cursor = new Cursor(0, SeekOrigin.Begin);

                _logger.Information($"Found gap of size {size} bytes at offset {cursor.Offset}");

                return true;
            }

            // 4. Check other gaps
            // We should find minimal gap but bigger or equals in size
            var minimalGapSize = int.MaxValue;
            var minimalGapCursor = default(Cursor);
            var found = false;
            for (var i = 0; i < orderedDescriptors.Length - 1; i++)
            {
                var current = orderedDescriptors[i];
                var next = orderedDescriptors[i + 1];
                var currentEnd = current.Value.DataOffset + current.Value.DataLength;
                var nextStart = next.Value.DataOffset;

                // If we have gap with specific size or between files
                var currentGapSize = nextStart - currentEnd;
                if (currentGapSize >= size && currentGapSize < minimalGapSize)
                {
                    minimalGapSize = currentGapSize;
                    minimalGapCursor = new Cursor(currentEnd, SeekOrigin.Begin);

                    // We should know that we found at least one correct gap
                    found = true;
                }
            }

            if (found)
            {
                cursor = minimalGapCursor;

                _logger.Information($"Found gap of size {size} bytes at offset {cursor.Offset}");

                return true;
            }

            _logger.Information($"Gap of size {size} bytes not found");

            return false;
        }
    }
}