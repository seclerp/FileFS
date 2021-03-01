using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// Base entry repository.
    /// </summary>
    public class EntryRepository : IEntryRepository
    {
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly IEntryDescriptorRepository _entryDescriptorRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryRepository"/> class.
        /// </summary>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="entryDescriptorRepository">File descriptor repository.</param>
        /// <param name="logger">Logger instance.</param>
        public EntryRepository(
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            IEntryDescriptorRepository entryDescriptorRepository,
            ILogger logger)
        {
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _entryDescriptorRepository = entryDescriptorRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Rename(string currentEntryName, string newEntryName)
        {
            Move(currentEntryName, newEntryName);
        }

        /// <inheritdoc />
        public void Delete(string entryName)
        {
            // 1. Find last descriptor
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var lastDescriptorCursor = new Cursor(
                -FilesystemDescriptor.BytesTotal -
                (filesystemDescriptor.EntryDescriptorsCount *
                 filesystemDescriptor.EntryDescriptorLength),
                SeekOrigin.End);
            var lastDescriptor = _entryDescriptorRepository.Read(lastDescriptorCursor).Value;

            // 2. Find current descriptor
            var descriptorItem = _entryDescriptorRepository.Find(entryName);
            var cursor = descriptorItem.Cursor;

            // 3. Save last descriptor in new empty space (perform swap with last)
            _entryDescriptorRepository.Write(new StorageItem<EntryDescriptor>(lastDescriptor, cursor));

            // 4. Decrease count of descriptors
            IncreaseEntriesCount(-1);
        }

        /// <inheritdoc />
        public virtual bool Exists(string entryName)
        {
            return _entryDescriptorRepository.Exists(entryName);
        }

        /// <inheritdoc />
        public void Move(string fromName, string toName)
        {
            var newParentName = toName.GetParentFullName();
            var newParent = _entryDescriptorRepository.Find(newParentName).Value;
            var descriptorItem = _entryDescriptorRepository.Find(fromName);

            var newName = toName.GetParentFullName().CombineWith(toName.GetShortName());

            var updatedDescriptor = new EntryDescriptor(
                descriptorItem.Value.Id,
                newParent.Id,
                newName,
                descriptorItem.Value.Type,
                descriptorItem.Value.CreatedOn,
                descriptorItem.Value.UpdatedOn,
                descriptorItem.Value.DataOffset,
                descriptorItem.Value.DataLength);
            var cursor = descriptorItem.Cursor;

            _entryDescriptorRepository.Write(new StorageItem<EntryDescriptor>(updatedDescriptor, cursor));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<FileFsEntryInfo> GetEntriesInfo(string directoryName)
        {
            return _entryDescriptorRepository
                .ReadChildren(directoryName)
                .Select(info => new FileFsEntryInfo(
                    info.Value.Name,
                    info.Value.Type,
                    info.Value.DataLength,
                    info.Value.CreatedOn.FromUnixTime(),
                    info.Value.UpdatedOn.FromUnixTime()))
                .ToArray();
        }

        /// <summary>
        /// Increments count of entries in filesystem descriptor.
        /// </summary>
        /// <param name="relativeAmount">Relative amount to increase entries count to.</param>
        protected void IncreaseEntriesCount(int relativeAmount)
        {
            _logger.Information("Start updating filesystem descriptor");

            _filesystemDescriptorAccessor.Update(entryDescriptorsCountUpdater: value => value + relativeAmount);

            _logger.Information("Done updating filesystem descriptor");
        }
    }
}