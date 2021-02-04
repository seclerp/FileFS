using System.IO;
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
        private readonly IStorageOptimizer _optimizer;

        public FileAllocator(IStorageConnection storageConnection, IFilesystemDescriptorRepository filesystemDescriptorRepository, IStorageOptimizer optimizer)
        {
            _storageConnection = storageConnection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _optimizer = optimizer;
        }

        public Cursor AllocateFile(int dataSize)
        {
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
    }
}