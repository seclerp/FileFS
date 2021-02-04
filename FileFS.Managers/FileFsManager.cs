using System.Collections.Generic;
using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFS.Managers.Abstractions;
using FileFS.Managers.Models;

namespace FileFS.Managers
{
    public class FileFsManager : IFileFsManager
    {
        private readonly IFileAllocator _allocator;
        private readonly IFileDataRepository _fileDataRepository;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;

        public FileFsManager(
            IFileAllocator allocator,
            IFileDataRepository fileDataRepository,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            IFileDescriptorRepository fileDescriptorRepository)
        {
            _allocator = allocator;
            _fileDataRepository = fileDataRepository;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _fileDescriptorRepository = fileDescriptorRepository;
        }

        public void Create(string fileName, byte[] contentBytes)
        {
            // 1. Allocate space
            var allocatedCursor = _allocator.AllocateFile(contentBytes.Length);

            // 2. Write file descriptor
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var fileDescriptor = new FileDescriptor(fileName, allocatedCursor.Offset, contentBytes.Length);
            var fileDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.FileDescriptorsCount *
                                        filesystemDescriptor.FileDescriptorLength)
                                       - filesystemDescriptor.FileDescriptorLength;
            _fileDescriptorRepository.Write(fileDescriptor, fileDescriptorOffset);


            // 3. Update filesystem descriptor
            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength,
                filesystemDescriptor.FileDescriptorsCount + 1,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);

            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);

            // 4. Write data
            _fileDataRepository.Write(contentBytes, allocatedCursor.Offset);
        }

        public byte[] Read(string fileName)
        {
            // 1. Find descriptor
            var descriptor = FindDescriptor(fileName, out _);

            // 2. Read data by given offset from descriptor
            var dataBytes = _fileDataRepository.Read(descriptor.DataOffset, descriptor.DataLength);

            return dataBytes;
        }

        public void Rename(string oldFilename, string newFilename)
        {
            // 1. Find descriptor
            var descriptor = FindDescriptor(oldFilename, out var offset);

            // 2. Create descriptor with new filename
            var newDescriptor = new FileDescriptor(newFilename, descriptor.DataOffset, descriptor.DataLength);

            // 3. Write new descriptor
            _fileDescriptorRepository.Write(newDescriptor, offset);
        }

        public void Delete(string fileName)
        {
            // 1. Find last descriptor
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var lastDescriptorOffset =
                -FilesystemDescriptor.BytesTotal -
                (filesystemDescriptor.FileDescriptorsCount *
                 filesystemDescriptor.FileDescriptorLength);
            var lastDescriptor = _fileDescriptorRepository.Read(lastDescriptorOffset);

            // 2. Find current descriptor
            FindDescriptor(fileName, out var offsetToBeDeleted);

            // 3. Save last descriptor in new empty space (perform swap with last)
            _fileDescriptorRepository.Write(lastDescriptor, offsetToBeDeleted);

            // 4. Decrease count of descriptors
            filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength,
                filesystemDescriptor.FileDescriptorsCount - 1,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);
            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);
        }

        public bool Exists(string fileName)
        {
            return TryFindDescriptor(fileName, out _, out _);
        }

        public IReadOnlyCollection<EntryInfo> List()
        {
            return _fileDescriptorRepository
                .ReadAll()
                .Select(descriptor => new EntryInfo(descriptor.FileName, descriptor.DataLength))
                .ToArray();
        }

        private FileDescriptor FindDescriptor(string fileName, out int offset)
        {
            if (!TryFindDescriptor(fileName, out var descriptor, out var descriptorOffset))
            {
                // TODO: Throw FileNotFoundException
            }

            offset = descriptorOffset;
            return descriptor;
        }

        private bool TryFindDescriptor(string fileName, out FileDescriptor descriptor, out int descriptorOffset)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            descriptor = default;
            descriptorOffset = default;
            var found = false;
            for (var offset = startFromOffset; offset >= endOffset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var currentDescriptor = _fileDescriptorRepository.Read(offset);
                if (currentDescriptor.FileName == fileName)
                {
                    descriptor = currentDescriptor;
                    descriptorOffset = offset;
                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}