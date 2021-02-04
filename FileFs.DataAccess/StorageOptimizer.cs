using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFs.DataAccess
{
    public class StorageOptimizer : IStorageOptimizer
    {
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly IFileDataRepository _dataRepository;

        public StorageOptimizer(IFileDescriptorRepository fileDescriptorRepository, IFileDataRepository dataRepository)
        {
            _fileDescriptorRepository = fileDescriptorRepository;
            _dataRepository = dataRepository;
        }

        public void Optimize()
        {
            // 1. Get all descriptors
            var descriptors = _fileDescriptorRepository.ReadAll();

            // 2. Sort by offset
            var orderedDescriptors = descriptors.OrderBy(descriptor => descriptor.Value.DataOffset).ToArray();

            // Move first
            if (orderedDescriptors.Length > 0 && orderedDescriptors[0].Value.DataOffset != 0)
            {
                var newOffset = 0;
                var newStorageItem = CopyFile(orderedDescriptors[0].Value, orderedDescriptors[0].Cursor, newOffset);
                orderedDescriptors[0] = newStorageItem;
            }

            // 3. Iterate over all descriptors to find gaps
            for (var i = 0; i < descriptors.Count - 1; i++)
            {
                var first = orderedDescriptors[i];
                var second = orderedDescriptors[i + 1];
                var firstEnd = first.Value.DataOffset + first.Value.DataLength;
                var secondStart = second.Value.DataOffset;

                // 4. Found a gap, write data of second right after first
                if (secondStart - firstEnd > 0)
                {
                    var newOffset = firstEnd;
                    var newStorageItem = CopyFile(second.Value, second.Cursor, newOffset);
                    orderedDescriptors[i + 1] = newStorageItem;
                }
            }
        }

        private StorageItem<FileDescriptor> CopyFile(FileDescriptor fileDescriptor, Cursor cursor, int destinationOffset)
        {
            var newDescriptor = new FileDescriptor(fileDescriptor.FileName, destinationOffset, fileDescriptor.DataLength);
            var newStorageItem = new StorageItem<FileDescriptor>(ref newDescriptor, ref cursor);

            _dataRepository.Copy(fileDescriptor.DataOffset, destinationOffset, fileDescriptor.DataLength);
            _fileDescriptorRepository.Write(newStorageItem);

            return newStorageItem;
        }
    }
}