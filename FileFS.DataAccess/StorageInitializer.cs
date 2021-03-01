using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// FileStream based storage initializer implementation.
    /// </summary>
    public class StorageInitializer : IStorageInitializer
    {
        private readonly IStorageConnection _storageConnection;
        private readonly IStorageOperationLocker _storageOperationLocker;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageInitializer"/> class.
        /// </summary>
        /// <param name="storageConnection">Storage connection instance.</param>
        /// <param name="storageOperationLocker">Storage operation locker instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="entryDescriptorRepository">Entry descriptor repository instance.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageInitializer(
            IStorageConnection storageConnection,
            IStorageOperationLocker storageOperationLocker,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IEntryDescriptorRepository entryDescriptorRepository,
            ILogger logger)
        {
            _storageConnection = storageConnection;
            _storageOperationLocker = storageOperationLocker;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _entryDescriptorRepository = entryDescriptorRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNonValidException">Throws when fileSize less than reserved bytes for filesystem descriptor.</exception>
        /// <exception cref="ArgumentNonValidException">Throws when fileNameLength less or equals to 0.</exception>
        public void Initialize(int fileSize, int fileNameLength)
        {
            var minimalSize = FilesystemDescriptor.BytesTotal + EntryDescriptor.BytesWithoutFilename + fileNameLength;
            if (fileSize < minimalSize)
            {
                throw new ArgumentNonValidException($"Value '{nameof(fileSize)}' cannot be less than reserved bytes for filesystem descriptor and root folder ({minimalSize})");
            }

            if (fileNameLength <= 0)
            {
                throw new ArgumentNonValidException($"Value '{nameof(fileNameLength)}'cannot be less or equals to 0");
            }

            _storageOperationLocker.GlobalLock(() =>
            {
                _logger.Information($"Start storage initialization process, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");

                // Simply initialize new empty storage file
                _storageConnection.SetSize(fileSize);

                _filesystemDescriptorAccessor.Update(_ => 0, _ => 1, _ => fileNameLength + EntryDescriptor.BytesWithoutFilename);

                _logger.Information("Filesystem descriptor initialized");

                _logger.Information("Start creation of root directory");

                CreateRootDirectory();

                _logger.Information("Root directory was created");

                _logger.Information($"Done storage initialization process, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");
            });
        }

        private void CreateRootDirectory()
        {
            var rootDirectoryId = Guid.NewGuid();
            var createdOn = DateTime.UtcNow.ToUnixTime();
            var updatedOn = createdOn;
            var rootDirectory = new EntryDescriptor(
                rootDirectoryId,
                rootDirectoryId,
                "/",
                EntryType.Directory,
                createdOn,
                updatedOn,
                0,
                0);

            // Write root directory descriptor
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var cursor = new Cursor(-FilesystemDescriptor.BytesTotal - filesystemDescriptor.EntryDescriptorLength, SeekOrigin.End);
            _entryDescriptorRepository.Write(new StorageItem<EntryDescriptor>(rootDirectory, cursor));
        }
    }
}