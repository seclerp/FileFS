using System.IO;
using System.Text;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Serializers
{
    /// <summary>
    /// Filesystem descriptor serializer implementation.
    /// </summary>
    public class FilesystemDescriptorSerializer : ISerializer<FilesystemDescriptor>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemDescriptorSerializer"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public FilesystemDescriptorSerializer(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public FilesystemDescriptor FromBytes(byte[] buffer)
        {
            _logger.Information("Creating memory stream to read filesystem descriptor from buffer");

            using var stream = new MemoryStream(buffer);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            _logger.Information("Reading filesystem descriptor data");

            var filesDataLength = reader.ReadInt32();
            var fileDescriptorsCount = reader.ReadInt32();
            var fileDescriptorLength = reader.ReadInt32();

            _logger.Information("Done reading filesystem descriptor data");

            return new FilesystemDescriptor(filesDataLength, fileDescriptorsCount, fileDescriptorLength);
        }

        /// <inheritdoc />
        public byte[] ToBytes(FilesystemDescriptor model)
        {
            _logger.Information("Creating new buffer and memory stream to write filesystem descriptor to buffer");

            var buffer = new byte[FilesystemDescriptor.BytesTotal];
            using var stream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            _logger.Information("Writing filesystem descriptor data");

            writer.Write(model.FilesDataLength);
            writer.Write(model.EntryDescriptorsCount);
            writer.Write(model.EntryDescriptorLength);

            _logger.Information("Done writing filesystem descriptor data");

            return buffer;
        }
    }
}