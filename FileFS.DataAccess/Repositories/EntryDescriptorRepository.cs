using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// File descriptor repository implementation.
    /// </summary>
    public class EntryDescriptorRepository : IEntryDescriptorRepository
    {
        private readonly IStorageConnection _connection;
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ISerializer<EntryDescriptor> _serializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDescriptorRepository"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="serializer">File descriptor serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public EntryDescriptorRepository(
            IStorageConnection connection,
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ISerializer<EntryDescriptor> serializer,
            ILogger logger)
        {
            _connection = connection;
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public StorageItem<EntryDescriptor> Read(Guid id)
        {
            if (TryFindInternal(descriptor => descriptor.Id == id, out var item))
            {
                throw new EntryDescriptorNotFound(id);
            }

            return item;
        }

        /// <inheritdoc />
        public StorageItem<EntryDescriptor> Read(Cursor cursor)
        {
            _logger.Information("Start file descriptor data reading process");

            _logger.Information("Retrieving info about file descriptor length");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var length = filesystemDescriptor.EntryDescriptorLength;

            _logger.Information("Reading file descriptor data");

            var data = _connection.PerformRead(cursor, length);
            var descriptor = _serializer.FromBytes(data);

            _logger.Information("Done reading file descriptor data");

            return new StorageItem<EntryDescriptor>(in descriptor, in cursor);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<StorageItem<EntryDescriptor>> ReadAll()
        {
            return FindManyInternal(_ => true);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<StorageItem<EntryDescriptor>> ReadChildren(string entryName)
        {
            var entryDescriptor = Find(entryName).Value;
            var children = FindManyInternal(descriptor => descriptor.ParentId == entryDescriptor.Id);

            return children;
        }

        /// <inheritdoc />
        public void Write(StorageItem<EntryDescriptor> item)
        {
            _logger.Information("Start file descriptor data writing process");

            var data = _serializer.ToBytes(item.Value);

            _connection.PerformWrite(item.Cursor, data);

            _logger.Information("Done writing file descriptor data");
        }

        /// <inheritdoc />
        public StorageItem<EntryDescriptor> Find(string entryName)
        {
            if (!TryFind(entryName, out var item))
            {
                throw new EntryDescriptorNotFound(entryName);
            }

            return item;
        }

        /// <inheritdoc />
        public bool TryFind(string entryName, out StorageItem<EntryDescriptor> item)
        {
            return TryFindInternal(descriptor => descriptor.Name == entryName, out item);
        }

        /// <inheritdoc />
        public bool Exists(string entryName)
        {
            return TryFindInternal(descriptor => descriptor.Name == entryName, out _);
        }

        private static CursorRange GetDescriptorsRange(in FilesystemDescriptor filesystemDescriptor)
        {
            const SeekOrigin origin = SeekOrigin.End;

            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.EntryDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.EntryDescriptorsCount *
                             filesystemDescriptor.EntryDescriptorLength);

            var begin = new Cursor(startFromOffset, origin);
            var end = new Cursor(endOffset, origin);

            return new CursorRange(begin, end);
        }

        private bool TryFindInternal(Func<EntryDescriptor, bool> selector, out StorageItem<EntryDescriptor> item)
        {
            var items = FindManyInternal(selector);
            item = items.FirstOrDefault();

            return items.Any();
        }

        private IReadOnlyCollection<StorageItem<EntryDescriptor>> FindManyInternal(Func<EntryDescriptor, bool> selector)
        {
            var items = new LinkedList<StorageItem<EntryDescriptor>>();
            _logger.Information("Start file descriptor search process");

            _logger.Information("Retrieving info about file descriptors from filesystem descriptor");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var descriptorsCursorRange = GetDescriptorsRange(filesystemDescriptor);

            _logger.Information("Searching for specific file descriptors");

            for (var offset = descriptorsCursorRange.Begin.Offset; offset >= descriptorsCursorRange.End.Offset; offset -= filesystemDescriptor.EntryDescriptorLength)
            {
                var cursor = new Cursor(offset, SeekOrigin.End);
                var data = _connection.PerformRead(cursor, filesystemDescriptor.EntryDescriptorLength);
                var currentDescriptor = _serializer.FromBytes(data);

                if (selector(currentDescriptor))
                {
                    _logger.Information("Specific descriptors found");

                    items.AddLast(new StorageItem<EntryDescriptor>(currentDescriptor, cursor));
                }
            }

            return items;
        }
    }
}