using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

namespace FileFs.DataAccess
{
    public class StorageOptimizer : IStorageOptimizer
    {
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly IFileDataRepository _dataRepository;
        private readonly ILogger<StorageOptimizer> _logger;

        public StorageOptimizer(IFileDescriptorRepository fileDescriptorRepository, IFileDataRepository dataRepository, ILogger<StorageOptimizer> logger)
        {
            _fileDescriptorRepository = fileDescriptorRepository;
            _dataRepository = dataRepository;
            _logger = logger;
        }

        public void Optimize()
        {
            _logger.LogInformation("Start optimization process");

            var dataItemsMoved = 0;
            var bytesOptimized = 0;

            // 1. Get all descriptors
            var descriptors = _fileDescriptorRepository.ReadAll();

            _logger.LogInformation($"There is {descriptors.Count} descriptors found");

            // 2. Sort by offset
            var orderedDescriptors = descriptors.OrderBy(descriptor => descriptor.Value.DataOffset).ToArray();

            // 3a. Move first
            if (orderedDescriptors.Length > 0 && orderedDescriptors[0].Value.DataOffset != 0)
            {
                var gapSize = orderedDescriptors[0].Value.DataOffset - 1;
                ProcessGap(orderedDescriptors, 0, 0);
                dataItemsMoved++;
                bytesOptimized += gapSize;
            }

            // 3b. Iterate over all descriptors to find gaps
            for (var i = 0; i < descriptors.Count - 1; i++)
            {
                var current = orderedDescriptors[i];
                var next = orderedDescriptors[i + 1];
                var currentEnd = current.Value.DataOffset + current.Value.DataLength;
                var nextStart = next.Value.DataOffset;

                // 4. Found a gap, write data of second right after first
                if (nextStart - currentEnd > 0)
                {
                    var gapSize = nextStart - currentEnd;
                    ProcessGap(orderedDescriptors, i + 1, currentEnd);
                    dataItemsMoved++;
                    bytesOptimized += gapSize;
                }
            }

            _logger.LogInformation($"Optimization process completed, {dataItemsMoved} items moved, {bytesOptimized} bytes optimized");
        }

        private void ProcessGap(StorageItem<FileDescriptor>[] orderedDescriptors, int descriptorIndex, int gapOffset)
        {
            var movingDataDescriptorItem = orderedDescriptors[descriptorIndex];

            _logger.LogInformation($"Found gap of size {movingDataDescriptorItem.Value.DataOffset - 1} on offset {gapOffset}");

            var newStorageItem = CopyFile(movingDataDescriptorItem.Value, movingDataDescriptorItem.Cursor, gapOffset);
            orderedDescriptors[descriptorIndex] = newStorageItem;
        }

        private StorageItem<FileDescriptor> CopyFile(FileDescriptor fileDescriptor, Cursor cursor, int destinationOffset)
        {
            var newDescriptor = new FileDescriptor(fileDescriptor.FileName, destinationOffset, fileDescriptor.DataLength);
            var newStorageItem = new StorageItem<FileDescriptor>(ref newDescriptor, ref cursor);

            _logger.LogInformation($"Moving {fileDescriptor.DataLength} bytes of data from {fileDescriptor.DataOffset} to {destinationOffset}");

            _dataRepository.Copy(fileDescriptor.DataOffset, destinationOffset, fileDescriptor.DataLength);
            _fileDescriptorRepository.Write(newStorageItem);

            _logger.LogInformation($"{fileDescriptor.DataLength} bytes of data moved to offset {destinationOffset}");

            return newStorageItem;
        }
    }
}