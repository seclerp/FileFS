using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Abstractions;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Memory.Abstractions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// File repository implementation.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        private readonly IStorageConnection _connection;
        private readonly IFileAllocator _allocator;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRepository"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="allocator">File allocator instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="fileDescriptorRepository">File descriptor repository.</param>
        /// <param name="logger">Logger instance.</param>
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

        /// <inheritdoc />
        public void Create(FileEntry file)
        {
            CreateInternal(file, cursor => WriteFileData(cursor, file.Data));
        }

        /// <inheritdoc />
        public void Create(StreamedFileEntry streamedFile)
        {
            CreateInternal(streamedFile, cursor => WriteStreamedFileData(cursor, streamedFile.DataStream, streamedFile.DataLength));
        }

        /// <inheritdoc />
        public void Update(FileEntry file)
        {
            UpdateInternal(file, cursor => WriteFileData(cursor, file.Data));
        }

        /// <inheritdoc />
        public void Update(StreamedFileEntry streamedFile)
        {
            UpdateInternal(streamedFile, cursor => WriteStreamedFileData(cursor, streamedFile.DataStream, streamedFile.DataLength));
        }

        /// <inheritdoc />
        public FileEntry Read(string fileName)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(fileName);

            // 2. Read data by given offset from descriptor
            var dataBytes = ReadFileData(new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin), descriptorItem.Value.DataLength);

            return new FileEntry(fileName, dataBytes);
        }

        /// <inheritdoc />
        public void Read(string fileName, Stream destinationStream)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(fileName);

            // 2. Read data by given offset from descriptor
            ReadStreamedFileData(new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin), descriptorItem.Value.DataLength, destinationStream);
        }

        /// <inheritdoc />
        public void Rename(string currentFilename, string newFilename)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(currentFilename);

            // 2. Create descriptor with new filename
            var newDescriptor = new FileDescriptor(
                newFilename,
                descriptorItem.Value.CreatedOn,
                descriptorItem.Value.UpdatedOn,
                descriptorItem.Value.DataOffset,
                descriptorItem.Value.DataLength);
            var cursor = descriptorItem.Cursor;

            // 3. Write new descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(in newDescriptor, in cursor));
        }

        /// <inheritdoc />
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
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(in lastDescriptor, in cursor));

            // 4. Decrease count of descriptors
            var updatedFilesystemDescriptor =
                filesystemDescriptor.WithFileDescriptorsCount(filesystemDescriptor.FileDescriptorsCount - 1);

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);
        }

        /// <inheritdoc />
        public bool Exists(string fileName)
        {
            return _fileDescriptorRepository.Exists(fileName);
        }

        /// <inheritdoc />
        public IEnumerable<FileEntryInfo> GetAllFilesInfo()
        {
            return _fileDescriptorRepository
                .ReadAll()
                .Select(info => new FileEntryInfo(
                    info.Value.FileName,
                    info.Value.DataLength,
                    info.Value.CreatedOn.FromUnixTime(),
                    info.Value.UpdatedOn.FromUnixTime()))
                .ToArray();
        }

        private void CreateInternal(IFileEntry file, Action<Cursor> writeAction)
        {
            _logger.Information($"Start file create process, filename {file.FileName}, bytes count {file.DataLength}");

            // 1. Allocate space
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;

            // 2. Write new file descriptor
            var allocatedDataCursor = WriteFileDescriptor(file, filesystemDescriptor);

            // 3. Update filesystem descriptor
            UpdateFilesystemDescriptor(filesystemDescriptor);

            // 4. Write data
            writeAction(allocatedDataCursor);

            _logger.Information($"File {file.FileName} was created");
        }

        private void UpdateFilesystemDescriptor(FilesystemDescriptor filesystemDescriptor)
        {
            _logger.Information("Start updating filesystem descriptor");

            var updatedFilesystemDescriptor =
                filesystemDescriptor.WithFileDescriptorsCount(filesystemDescriptor.FileDescriptorsCount + 1);

            _filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);

            _logger.Information("Done updating filesystem descriptor");
        }

        private Cursor WriteFileDescriptor(IFileEntry file, FilesystemDescriptor filesystemDescriptor)
        {
            _logger.Information($"Start writing file descriptor for filename {file.FileName}");

            var allocatedCursor = _allocator.AllocateFile(file.DataLength);

            var createdOn = DateTime.UtcNow.ToUnixTime();
            var updatedOn = createdOn;
            var fileDescriptor = new FileDescriptor(file.FileName, createdOn, updatedOn, allocatedCursor.Offset, file.DataLength);
            var fileDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.FileDescriptorsCount *
                                        filesystemDescriptor.FileDescriptorLength)
                                       - filesystemDescriptor.FileDescriptorLength;

            var origin = SeekOrigin.End;
            var cursor = new Cursor(fileDescriptorOffset, origin);

            var storageItem = new StorageItem<FileDescriptor>(in fileDescriptor, in cursor);

            _fileDescriptorRepository.Write(storageItem);

            _logger.Information($"Done writing file descriptor for filename {file.FileName}");

            return allocatedCursor;
        }

        private void UpdateInternal(IFileEntry file, Action<Cursor> writeAction)
        {
            // 1. Find descriptor
            var descriptorItem = _fileDescriptorRepository.Find(file.FileName);

            // 2. If new content size equals or smaller than was previously allocated to this file,
            // we don't need to allocate new space, only change length
            var allocatedCursor = file.DataLength <= descriptorItem.Value.DataLength
                ? new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin)
                : _allocator.AllocateFile(file.DataLength);

            // 3. Write file data
            writeAction(allocatedCursor);

            var updatedOn = DateTime.UtcNow.ToUnixTime();
            var updatedDescriptor = new FileDescriptor(
                descriptorItem.Value.FileName,
                descriptorItem.Value.CreatedOn,
                updatedOn,
                allocatedCursor.Offset,
                file.DataLength);
            var cursor = descriptorItem.Cursor;

            // 4. Write descriptor
            _fileDescriptorRepository.Write(new StorageItem<FileDescriptor>(in updatedDescriptor, in cursor));
        }

        private byte[] ReadFileData(Cursor cursor, int length)
        {
            return _connection.PerformRead(cursor, length);
        }

        private void ReadStreamedFileData(Cursor cursor, int length, Stream destinationStream)
        {
            _connection.PerformRead(cursor, length, destinationStream);
        }

        private void WriteFileData(Cursor cursor, byte[] data)
        {
            _connection.PerformWrite(cursor, data);
        }

        private void WriteStreamedFileData(Cursor cursor, Stream dataStream, int length)
        {
            _connection.PerformWrite(cursor, length, dataStream);
        }
    }
}