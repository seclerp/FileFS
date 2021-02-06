using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Extensions;
using FileFs.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFs.DataAccess.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IStorageConnection _connection;
        private readonly IFileAllocator _allocator;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly ILogger _logger;

        public FileRepository(
            IStorageConnection connection,
            IFileAllocator allocator,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IFileDescriptorRepository fileDescriptorRepository,
            ILogger logger)
        {
            _connection = connection;
            _allocator = allocator;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _fileDescriptorRepository = fileDescriptorRepository;
            _logger = logger;
        }

        public void Create(FileEntry file)
        {
            _logger.Information($"Start file create process, filename {file.FileName}, bytes count {file.Content.Length}");

            // 1. Allocate space
            var allocatedCursor = _allocator.AllocateFile(file.Content.Length);

            // 2. Write file descriptor
            _logger.Information($"Start writing file descriptor for filename {file.FileName}");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var fileDescriptor = new FileDescriptor(file.FileName, allocatedCursor.Offset, file.Content.Length);
            var fileDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.FileDescriptorsCount *
                                        filesystemDescriptor.FileDescriptorLength)
                                       - filesystemDescriptor.FileDescriptorLength;

            var origin = SeekOrigin.End;
            var cursor = new Cursor(fileDescriptorOffset, origin);

            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref fileDescriptor, ref cursor));

            _logger.Information($"Done writing file descriptor for filename {file.FileName}");

            _logger.Information("Start updating filesystem descriptor");

            // 3. Update filesystem descriptor
            var updatedFilesystemDescriptor =
                filesystemDescriptor.WithFileDescriptorsCount(filesystemDescriptor.FileDescriptorsCount + 1);

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);

            _logger.Information("Done updating filesystem descriptor");

            // 4. Write data
            WriteFileData(allocatedCursor.Offset, file.Content);

            _logger.Information($"File {file.FileName} was created");
        }

        public void Update(FileEntry file)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(file.FileName);

            // 2. If new content size equals or smaller than was previously allocated to this file,
            // we don't need to allocate new space, only change length
            var allocatedOffset = file.Content.Length <= descriptorItem.Value.DataLength
                ? descriptorItem.Value.DataOffset
                : _allocator.AllocateFile(file.Content.Length).Offset;

            // 3. Write file data
            WriteFileData(allocatedOffset, file.Content);

            var updatedDescriptor = new FileDescriptor(
                descriptorItem.Value.FileName,
                allocatedOffset,
                file.Content.Length);
            var cursor = descriptorItem.Cursor;

            // 4. Write descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref updatedDescriptor, ref cursor));
        }

        public FileEntry Read(string fileName)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(fileName);

            // 2. Read data by given offset from descriptor
            var dataBytes = ReadFileData(descriptorItem.Value.DataOffset, descriptorItem.Value.DataLength);

            return new FileEntry(fileName, dataBytes);
        }

        public void Rename(string oldFilename, string newFilename)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(oldFilename);

            // 2. Create descriptor with new filename
            var newDescriptor = new FileDescriptor(newFilename, descriptorItem.Value.DataOffset, descriptorItem.Value.DataLength);
            var cursor = descriptorItem.Cursor;

            // 3. Write new descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref newDescriptor, ref cursor));
        }

        public void Delete(string fileName)
        {
            // 1. Find last descriptor
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var lastDescriptorOffset =
                -FilesystemDescriptor.BytesTotal -
                (filesystemDescriptor.FileDescriptorsCount *
                 filesystemDescriptor.FileDescriptorLength);
            var lastDescriptor = _fileDescriptorRepository.Read(lastDescriptorOffset).Value;

            // 2. Find current descriptor
            var descriptorItem = _fileDescriptorRepository.Find(fileName);
            var cursor = descriptorItem.Cursor;

            // 3. Save last descriptor in new empty space (perform swap with last)
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(ref lastDescriptor, ref cursor));

            // 4. Decrease count of descriptors
            var updatedFilesystemDescriptor =
                filesystemDescriptor.WithFileDescriptorsCount(filesystemDescriptor.FileDescriptorsCount - 1);

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);
        }

        public bool Exists(string fileName)
        {
            return _fileDescriptorRepository.Exists(fileName);
        }

        public IReadOnlyCollection<FileEntryInfo> GetAllFilesInfo()
        {
            return _fileDescriptorRepository
                .ReadAll()
                .Select(info => new FileEntryInfo(info.Value.FileName, info.Value.DataLength))
                .ToArray();
        }

        private byte[] ReadFileData(int offset, int length)
        {
            var origin = SeekOrigin.Begin;

            return _connection.PerformRead(new Cursor(offset, origin), length);
        }

        private void WriteFileData(int offset, byte[] data)
        {
            var origin = SeekOrigin.Begin;

            _connection.PerformWrite(new Cursor(offset, origin), data);
        }
    }
}