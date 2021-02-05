using System.IO;
using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Exceptions;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFs.DataAccess
{
    public class FileAllocator : IFileAllocator
    {
        private readonly IStorageConnection _storageConnection;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly IStorageOptimizer _optimizer;

        public FileAllocator(
            IStorageConnection storageConnection,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            IFileDescriptorRepository fileDescriptorRepository,
            IStorageOptimizer optimizer)
        {
            _storageConnection = storageConnection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _fileDescriptorRepository = fileDescriptorRepository;
            _optimizer = optimizer;
        }

        public Cursor AllocateFile(int dataSize)
        {
            // 1. Try found existing gap of given size
            if (TryFindGap(dataSize, out var cursor))
            {
                return cursor;
            }

            // 2. No gaps with given size exists - try to allocate known empty space
            if (!CouldAllocate(dataSize))
            {
                // First try to optimize space
                _optimizer.Optimize();

                // Recheck
                if (!CouldAllocate(dataSize))
                {
                    throw new NotEnoughSpaceException($"There is no space for {dataSize} bytes.");
                }

                return PerformAllocate(dataSize);
            }

            return PerformAllocate(dataSize);
        }

        private bool CouldAllocate(int dataSize)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var overallSpace = _storageConnection.GetSize();
            var specialSpace = FilesystemDescriptor.BytesTotal +
                               (filesystemDescriptor.FileDescriptorsCount * filesystemDescriptor.FileDescriptorLength);

            var dataSpace = filesystemDescriptor.FilesDataLength;
            var remainingSpace = overallSpace - specialSpace - dataSpace;

            return remainingSpace >= dataSize;
        }

        private Cursor PerformAllocate(int dataSize)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var newDataOffset = filesystemDescriptor.FilesDataLength;
            var newDataCursor = new Cursor(newDataOffset, SeekOrigin.Begin);

            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength + dataSize,
                filesystemDescriptor.FileDescriptorsCount,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);

            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);

            return newDataCursor;
        }

        private bool TryFindGap(int size, out Cursor cursor)
        {
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

                    // We should now that we found at least one correct gap
                    found = true;
                }
            }

            if (found)
            {
                cursor = minimalGapCursor;
                return true;
            }

            return false;
        }
    }
}