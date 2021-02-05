using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFs.DataAccess;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFS.Managers.Abstractions;
using FileFS.Managers.Models;
using Microsoft.Extensions.Logging;

namespace FileFS.Managers
{
    public class FileFsManager : IFileFsManager
    {
        private readonly IFileAllocator _allocator;
        private readonly IFileDataRepository _fileDataRepository;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly IStorageOptimizer _storageOptimizer;
        private readonly IExternalFileManager _externalFileManager;
        private readonly ILogger<FileFsManager> _logger;

        public FileFsManager(
            IFileAllocator allocator,
            IFileDataRepository fileDataRepository,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            IFileDescriptorRepository fileDescriptorRepository,
            IStorageOptimizer storageOptimizer,
            IExternalFileManager externalFileManager,
            ILogger<FileFsManager> logger)
        {
            _allocator = allocator;
            _fileDataRepository = fileDataRepository;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _fileDescriptorRepository = fileDescriptorRepository;
            _storageOptimizer = storageOptimizer;
            _externalFileManager = externalFileManager;
            _logger = logger;
        }

        public void Create(string fileName, byte[] contentBytes)
        {
            _logger.LogInformation($"Start file create process, filename {fileName}, bytes count {contentBytes.Length}");

            // 1. Allocate space
            var allocatedCursor = _allocator.AllocateFile(contentBytes.Length);

            // 2. Write file descriptor
            _logger.LogInformation($"Start writing file descriptor for filename {fileName}");

            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var fileDescriptor = new FileDescriptor(fileName, allocatedCursor.Offset, contentBytes.Length);
            var fileDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.FileDescriptorsCount *
                                        filesystemDescriptor.FileDescriptorLength)
                                       - filesystemDescriptor.FileDescriptorLength;

            var origin = SeekOrigin.End;
            var cursor = new Cursor(fileDescriptorOffset, origin);

            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref fileDescriptor, ref cursor));

            _logger.LogInformation($"Done writing file descriptor for filename {fileName}");

            _logger.LogInformation("Start updating filesystem descriptor");

            // 3. Update filesystem descriptor
            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength,
                filesystemDescriptor.FileDescriptorsCount + 1,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);

            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);

            _logger.LogInformation("Done updating filesystem descriptor");

            // 4. Write data
            _fileDataRepository.Write(contentBytes, allocatedCursor.Offset);

            _logger.LogInformation($"File {fileName} was created");
        }

        public void Update(string fileName, byte[] newContentBytes)
        {
            // 1. Find descriptor
            var descriptorItem = FindDescriptor(fileName);

            // 2. If new content size equals or smaller than was previously allocated to this file,
            // we don't need to allocate new space, only change length
            var allocatedOffset = newContentBytes.Length <= descriptorItem.Value.DataLength
                ? descriptorItem.Value.DataOffset
                : _allocator.AllocateFile(newContentBytes.Length).Offset;

            // 3. Write file data
            _fileDataRepository.Write(newContentBytes, allocatedOffset);

            var updatedDescriptor = new FileDescriptor(
                descriptorItem.Value.FileName,
                allocatedOffset,
                newContentBytes.Length);
            var cursor = descriptorItem.Cursor;

            // 4. Write descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref updatedDescriptor, ref cursor));
        }

        public byte[] Read(string fileName)
        {
            // 1. Find descriptor
            var descriptorItem = FindDescriptor(fileName);

            // 2. Read data by given offset from descriptor
            var dataBytes = _fileDataRepository.Read(descriptorItem.Value.DataOffset, descriptorItem.Value.DataLength);

            return dataBytes;
        }

        public void Rename(string oldFilename, string newFilename)
        {
            // 1. Find descriptor
            var descriptorItem = FindDescriptor(oldFilename);

            // 2. Create descriptor with new filename
            var newDescriptor = new FileDescriptor(newFilename, descriptorItem.Value.DataOffset, descriptorItem.Value.DataLength);
            var cursor = descriptorItem.Cursor;

            // 3. Write new descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref newDescriptor, ref cursor));
        }

        public void Delete(string fileName)
        {
            // 1. Find last descriptor
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var lastDescriptorOffset =
                -FilesystemDescriptor.BytesTotal -
                (filesystemDescriptor.FileDescriptorsCount *
                 filesystemDescriptor.FileDescriptorLength);
            var lastDescriptor = _fileDescriptorRepository.Read(lastDescriptorOffset).Value;

            // 2. Find current descriptor
            var descriptorItem = FindDescriptor(fileName);
            var cursor = descriptorItem.Cursor;

            // 3. Save last descriptor in new empty space (perform swap with last)
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref lastDescriptor, ref cursor));

            // 4. Decrease count of descriptors
            filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var updatedFilesystemDescriptor = new FilesystemDescriptor(
                filesystemDescriptor.FilesDataLength,
                filesystemDescriptor.FileDescriptorsCount - 1,
                filesystemDescriptor.FileDescriptorLength,
                filesystemDescriptor.Version);
            _filesystemDescriptorRepository.Write(updatedFilesystemDescriptor);
        }

        public void Import(string externalPath, string fileName)
        {
            if (Exists(fileName))
            {
                // TODO: Throw FileAlreadyExistsException
            }

            if (File.Exists(externalPath))
            {
                // TODO: Throw ExternalFileAlreadyExistsException
            }

            // TODO: Use stream and buffering

            var contentBytes = _externalFileManager.Read(externalPath);

            Create(fileName, contentBytes);
        }

        public void Export(string fileName, string externalPath)
        {
            if (!Exists(fileName))
            {
                // TODO: Throw FileNotFoundException
            }

            if (!File.Exists(externalPath))
            {
                // TODO: Throw ExternalFileNotFoundException
            }

            // TODO: Use stream and buffering
            var contentBytes = Read(fileName);

            _externalFileManager.Write(externalPath, contentBytes);
        }

        public bool Exists(string fileName)
        {
            return TryFindDescriptor(fileName, out _);
        }

        public IReadOnlyCollection<EntryInfo> List()
        {
            return _fileDescriptorRepository
                .ReadAll()
                .Select(descriptor => new EntryInfo(descriptor.Value.FileName, descriptor.Value.DataLength))
                .ToArray();
        }

        public void ForceOptimize()
        {
            _storageOptimizer.Optimize();
        }

        private StorageItem<FileDescriptor> FindDescriptor(string fileName)
        {
            if (!TryFindDescriptor(fileName, out var descriptorItem))
            {
                // TODO: Throw FileNotFoundException
            }

            return descriptorItem;
        }

        private bool TryFindDescriptor(string fileName, out StorageItem<FileDescriptor> descriptorItem)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            descriptorItem = default;
            var found = false;
            for (var offset = startFromOffset; offset >= endOffset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var currentDescriptor = _fileDescriptorRepository.Read(offset);
                if (currentDescriptor.Value.FileName == fileName)
                {
                    descriptorItem = currentDescriptor;
                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}