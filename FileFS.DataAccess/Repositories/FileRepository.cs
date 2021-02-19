using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Abstractions;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// File repository implementation.
    /// </summary>
    public class FileRepository : EntryRepository, IFileRepository
    {
        private readonly IStorageConnection _connection;
        private readonly IFileAllocator _allocator;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRepository"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="allocator">File allocator instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="entryDescriptorRepository">File descriptor repository.</param>
        /// <param name="logger">Logger instance.</param>
        public FileRepository(
            IStorageConnection connection,
            IFileAllocator allocator,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IEntryDescriptorRepository entryDescriptorRepository,
            ILogger logger)
            : base(filesystemDescriptorAccessor, entryDescriptorRepository, logger)
        {
            _connection = connection;
            _allocator = allocator;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _entryDescriptorRepository = entryDescriptorRepository;
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
            var descriptorItem = _entryDescriptorRepository.Find(fileName);

            // 2. Read data by given offset from descriptor
            var dataBytes = ReadFileData(new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin), descriptorItem.Value.DataLength);

            return new FileEntry(fileName, descriptorItem.Value.ParentId, dataBytes);
        }

        /// <inheritdoc />
        public void Read(string fileName, Stream destinationStream)
        {
            // 1. Find descriptor
            var descriptorItem = _entryDescriptorRepository.Find(fileName);

            // 2. Read data by given offset from descriptor
            ReadStreamedFileData(new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin), descriptorItem.Value.DataLength, destinationStream);
        }

        /// <inheritdoc/>
        public override bool Exists(string entryName)
        {
            return _entryDescriptorRepository.TryFind(entryName, out var item)
                   && item.Value.Type is EntryType.File;
        }

        private void CreateInternal(IFileEntry file, Action<Cursor> writeAction)
        {
            _logger.Information($"Start file create process, filename {file.EntryName}, bytes count {file.DataLength}");

            // 1. Allocate space and write new file descriptor
            var allocatedDataCursor = WriteFileDescriptor(file);

            // 2. Update filesystem descriptor
            IncreaseEntriesCount(1);

            // 3. Write data
            writeAction(allocatedDataCursor);

            _logger.Information($"File {file.EntryName} was created");
        }

        private Cursor WriteFileDescriptor(IFileEntry file)
        {
            _logger.Information($"Start writing file descriptor for filename {file.EntryName}");

            var allocatedCursor = _allocator.AllocateFile(file.DataLength);

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var createdOn = DateTime.UtcNow.ToUnixTime();
            var updatedOn = createdOn;
            var id = Guid.NewGuid();
            var fileDescriptor = new EntryDescriptor(id, file.ParentEntryId, file.EntryName, EntryType.File, createdOn, updatedOn, allocatedCursor.Offset, file.DataLength);
            var fileDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.EntryDescriptorsCount *
                                        filesystemDescriptor.EntryDescriptorLength)
                                       - filesystemDescriptor.EntryDescriptorLength;

            var origin = SeekOrigin.End;
            var cursor = new Cursor(fileDescriptorOffset, origin);

            var storageItem = new StorageItem<EntryDescriptor>(in fileDescriptor, in cursor);

            _entryDescriptorRepository.Write(storageItem);

            _logger.Information($"Done writing file descriptor for filename {file.EntryName}");

            return allocatedCursor;
        }

        private void UpdateInternal(IFileEntry file, Action<Cursor> writeAction)
        {
            // 1. Find descriptor
            var descriptorItem = _entryDescriptorRepository.Find(file.EntryName);

            // 2. If new content size equals or smaller than was previously allocated to this file,
            // we don't need to allocate new space, only change length
            var allocatedCursor = file.DataLength <= descriptorItem.Value.DataLength
                ? new Cursor(descriptorItem.Value.DataOffset, SeekOrigin.Begin)
                : _allocator.AllocateFile(file.DataLength);

            // 3. Write file data
            writeAction(allocatedCursor);

            var updatedOn = DateTime.UtcNow.ToUnixTime();
            var updatedDescriptor = new EntryDescriptor(
                descriptorItem.Value.Id,
                descriptorItem.Value.ParentId,
                descriptorItem.Value.EntryName,
                EntryType.File,
                descriptorItem.Value.CreatedOn,
                updatedOn,
                allocatedCursor.Offset,
                file.DataLength);
            var cursor = descriptorItem.Cursor;

            // 4. Write descriptor
            _entryDescriptorRepository.Write(new StorageItem<EntryDescriptor>(in updatedDescriptor, in cursor));
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