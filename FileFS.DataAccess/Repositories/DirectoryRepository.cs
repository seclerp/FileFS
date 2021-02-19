using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// Directory repository implementation.
    /// </summary>
    public class DirectoryRepository : EntryRepository, IDirectoryRepository
    {
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryRepository"/> class.
        /// </summary>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="entryDescriptorRepository">File descriptor repository.</param>
        /// <param name="logger">Logger instance.</param>
        public DirectoryRepository(
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IEntryDescriptorRepository entryDescriptorRepository,
            ILogger logger)
            : base(filesystemDescriptorAccessor, entryDescriptorRepository, logger)
        {
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _entryDescriptorRepository = entryDescriptorRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public DirectoryEntry Find(string fullPath)
        {
            var descriptor = _entryDescriptorRepository.Find(fullPath);

            return new DirectoryEntry(
                descriptor.Value.Id,
                descriptor.Value.EntryName,
                descriptor.Value.ParentId);
        }

        /// <inheritdoc/>
        public void Create(string fullPath)
        {
            _logger.Information($"Start writing entry descriptor for directory {fullPath}");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var createdOn = DateTime.UtcNow.ToUnixTime();
            var updatedOn = createdOn;
            var id = Guid.NewGuid();
            var parentName = fullPath.GetParentFullName();
            var parentEntry = _entryDescriptorRepository.Find(parentName);
            var entryDescriptor = new EntryDescriptor(id, parentEntry.Value.Id, fullPath, EntryType.Directory, createdOn, updatedOn, 0, 0);
            var entryDescriptorOffset = -FilesystemDescriptor.BytesTotal -
                                       (filesystemDescriptor.EntryDescriptorsCount *
                                        filesystemDescriptor.EntryDescriptorLength)
                                       - filesystemDescriptor.EntryDescriptorLength;

            var origin = SeekOrigin.End;
            var cursor = new Cursor(entryDescriptorOffset, origin);

            var storageItem = new StorageItem<EntryDescriptor>(in entryDescriptor, in cursor);

            _entryDescriptorRepository.Write(storageItem);

            _logger.Information($"Done writing entry descriptor for directory {fullPath}");

            IncreaseEntriesCount(1);
        }

        /// <inheritdoc/>
        public override bool Exists(string entryName)
        {
            return _entryDescriptorRepository.TryFind(entryName, out var item)
                   && item.Value.Type is EntryType.Directory;
        }
    }
}