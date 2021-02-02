using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Exceptions;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFs.DataAccess
{
    public class FileAllocator : IFileAllocator
    {
        private readonly IFileFsConnection _connection;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;

        public FileAllocator(IFileFsConnection connection, IFilesystemDescriptorRepository filesystemDescriptorRepository)
        {
            _connection = connection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
        }

        public Cursor AllocateFile(int dataSize)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var overallSpace = _connection.GetSize();
            var specialSpace = FilesystemDescriptor.BytesTotal +
                               (filesystemDescriptor.FileDescriptorsCount * filesystemDescriptor.FileDescriptorLength);

            var dataSpace = filesystemDescriptor.FilesDataLength;
            var remainingSpace = overallSpace - specialSpace - dataSpace;

            if (remainingSpace < dataSize)
            {
                throw new NotEnoughSpaceException($"There is no space for {dataSize} bytes, got only {remainingSpace}");
            }

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