using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Serializers
{
    /// <summary>
    /// File descriptor serializer implementation.
    /// </summary>
    public class EntryDescriptorSerializer : ISerializer<EntryDescriptor>
    {
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDescriptorSerializer"/> class.
        /// </summary>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="logger">Logger instance.</param>
        public EntryDescriptorSerializer(
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ILogger logger)
        {
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public EntryDescriptor FromBytes(byte[] buffer)
        {
            _logger.Information("Creating memory stream to read file descriptor from buffer");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            using var stream = new MemoryStream(buffer);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            _logger.Information("Reading file descriptor data");

            var idBytes = reader.ReadGuidBytes();
            var id = new Guid(idBytes);
            var parentIdBytes = reader.ReadGuidBytes();
            var parentId = new Guid(parentIdBytes);
            var nameLength = reader.ReadInt32();
            var nameBytes = reader.ReadBytes(nameLength);
            var name = Encoding.UTF8.GetString(nameBytes);
            stream.Seek(filesystemDescriptor.EntryDescriptorLength - nameLength - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var typeByte = reader.ReadByte();
            var type = (EntryType)typeByte;
            var createdOn = reader.ReadInt64();
            var updatedOn = reader.ReadInt64();
            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();

            _logger.Information("Done reading file descriptor data");

            return new EntryDescriptor(id, parentId, name, type, createdOn, updatedOn, offset, length);
        }

        /// <inheritdoc />
        public byte[] ToBytes(EntryDescriptor model)
        {
            _logger.Information("Creating new buffer and memory stream to write file descriptor to buffer");

            var fileNameBytes = Encoding.UTF8.GetBytes(model.Name);
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var buffer = new byte[filesystemDescriptor.EntryDescriptorLength];
            using var stream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            _logger.Information("Writing file descriptor data");

            writer.Write(model.Id.ToByteArray());
            writer.Write(model.ParentId.ToByteArray());
            writer.Write(model.NameLength);
            writer.Write(fileNameBytes);
            writer.Seek(filesystemDescriptor.EntryDescriptorLength - fileNameBytes.Length - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write((byte)model.Type);
            writer.Write(model.CreatedOn);
            writer.Write(model.UpdatedOn);
            writer.Write(model.DataOffset);
            writer.Write(model.DataLength);

            _logger.Information("Done writing file descriptor data");

            return buffer;
        }
    }
}