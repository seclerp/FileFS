using System.IO;
using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Exceptions;
using FileFs.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

namespace FileFs.DataAccess
{
    public class FileAllocator : IFileAllocator
    {
        private readonly IStorageConnection _storageConnection;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly IStorageOptimizer _optimizer;
        private readonly ILogger<FileAllocator> _logger;

        public FileAllocator(
            IStorageConnection storageConnection,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            IFileDescriptorRepository fileDescriptorRepository,
            IStorageOptimizer optimizer,
            ILogger<FileAllocator> logger)
        {
            _storageConnection = storageConnection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _fileDescriptorRepository = fileDescriptorRepository;
            _optimizer = optimizer;
            _logger = logger;
        }

        public Cursor AllocateFile(int dataSize)
        {
            _logger.LogInformation($"Start memory allocation flow for {dataSize} bytes");

            // 1. Try find existing gap of given size
            if (TryFindGap(dataSize, out var cursor))
            {
                _logger.LogInformation($"Done performing allocation of {dataSize} bytes");

                return cursor;
            }

            // 2. No gaps with given size exists - try to allocate known empty space
            if (!CouldAllocate(dataSize))
            {
                _logger.LogInformation($"Failed to allocate new space with size of {dataSize} bytes, starting optimizer");

                // First try to optimize space
                _optimizer.Optimize();

                // Recheck
                if (!CouldAllocate(dataSize))
                {
                    throw new NotEnoughSpaceException($"There is no space in storage for {dataSize} bytes.");
                }

                return PerformAllocate(dataSize);
            }

            return PerformAllocate(dataSize);
        }

        private bool CouldAllocate(int dataSize)
        {
            _logger.LogInformation($"Checking possibility to allocate new {dataSize} bytes");

            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var overallSpace = _storageConnection.GetSize();
            var specialSpace = FilesystemDescriptor.BytesTotal +
                               (filesystemDescriptor.FileDescriptorsCount * filesystemDescriptor.FileDescriptorLength);

            var dataSpace = filesystemDescriptor.FilesDataLength;
            var remainingSpace = overallSpace - specialSpace - dataSpace;

            var couldAllocate = remainingSpace >= dataSize;

            _logger.LogInformation($"Could allocate decision: {couldAllocate}");

            return couldAllocate;
        }

        private Cursor PerformAllocate(int dataSize)
        {
            _logger.LogInformation($"Performing allocation for newly acquired space of {dataSize} bytes");

            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var newDataOffset = filesystemDescriptor.FilesDataLength;
            var newDataCursor = new Cursor(newDataOffset, SeekOrigin.Begin);

            _logger.LogInformation($"Space allocated at offset {newDataOffset}");
            _logger.LogInformation("Updating filesystem descriptor");

            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength + dataSize,
                filesystemDescriptor.FileDescriptorsCount,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);

            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);

            _logger.LogInformation("filesystem descriptor updated");
            _logger.LogInformation($"Done performing allocation of {dataSize} bytes");

            return newDataCursor;
        }

        private bool TryFindGap(int size, out Cursor cursor)
        {
            _logger.LogInformation($"Trying to find existing gap of {size} bytes");

            cursor = default;

            // 1. Get all descriptors
            var descriptors = _fileDescriptorRepository.ReadAll();

            // 2. Sort by offset
            var orderedDescriptors = descriptors.OrderBy(descriptor => descriptor.Value.DataOffset).ToArray();

            // 3. Check first gap
            if (orderedDescriptors.Length > 0 && orderedDescriptors[0].Value.DataOffset >= size)
            {
                // Start of storage is our gap
                cursor = new Cursor(0, SeekOrigin.Begin);

                _logger.LogInformation($"Found gap of size {size} bytes at offset {cursor.Offset}");

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
                    minimalGapCursor = new Cursor(currentEnd, current.Cursor.Origin);

                    // We should know that we found at least one correct gap
                    found = true;
                }
            }

            if (found)
            {
                cursor = minimalGapCursor;

                _logger.LogInformation($"Found gap of size {size} bytes at offset {cursor.Offset}");

                return true;
            }

            _logger.LogInformation($"Gap of size {size} bytes not found");

            return false;
        }
    }
}