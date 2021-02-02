using FileFs.DataAccess;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFS.Managers
{
    public class FileFsManager
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
            var descriptor = FindDescriptor(fileName);

            // 2. Read data by given offset from descriptor
            var dataBytes = _fileDataRepository.Read(descriptor.Offset, descriptor.Length);

            return dataBytes;
        }

        private FileDescriptor FindDescriptor(string fileName)
        {
            if (!TryFindDescriptor(fileName, out var descriptor))
            {
                // TODO: Throw FileNotFoundException
            }

            return descriptor;
        }

        private bool TryFindDescriptor(string fileName, out FileDescriptor descriptor)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            descriptor = default;
            var found = false;
            for (var offset = startFromOffset; offset >= endOffset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var currentDescriptor = _fileDescriptorRepository.Read(offset);
                if (currentDescriptor.FileName == fileName)
                {
                    descriptor = currentDescriptor;
                    found = true;
                }
            }

            return found;
        }
    }
}