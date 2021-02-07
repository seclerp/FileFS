using System.IO;
using System.Text;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Serializers
{
    /// <summary>
    /// File descriptor serializer implementation.
    /// </summary>
    public class FileDescriptorSerializer : ISerializer<FileDescriptor>
    {
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDescriptorSerializer"/> class.
        /// </summary>
        /// <param name="filesystemDescriptorAccessor">Filesystem descriptor accessor instance.</param>
        /// <param name="logger">Logger instance.</param>
        public FileDescriptorSerializer(
            IFilesystemDescriptorAccessor filesystemDescriptorAccessor,
            ILogger logger)
        {
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public FileDescriptor FromBuffer(byte[] buffer)
        {
            _logger.Information("Creating memory stream to read file descriptor from buffer");

            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            using var stream = new MemoryStream(buffer);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            _logger.Information("Reading file descriptor data");

            var stringLength = reader.ReadInt32();
            var fileNameBytes = reader.ReadBytes(stringLength);
            var fileName = Encoding.UTF8.GetString(fileNameBytes);
            stream.Seek(filesystemDescriptor.FileDescriptorLength - stringLength - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var createdOn = reader.ReadInt64();
            var updatedOn = reader.ReadInt64();
            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();

            _logger.Information("Done reading file descriptor data");

            return new FileDescriptor(fileName, createdOn, updatedOn, offset, length);
        }

        /// <inheritdoc />
        public byte[] ToBuffer(FileDescriptor model)
        {
            _logger.Information("Creating new buffer and memory stream to write file descriptor to buffer");

            var fileNameBytes = Encoding.UTF8.GetBytes(model.FileName);
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var buffer = new byte[filesystemDescriptor.FileDescriptorLength];
            using var stream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            _logger.Information("Writing file descriptor data");

            writer.Write(model.FileNameLength);
            writer.Write(fileNameBytes);
            writer.Seek(filesystemDescriptor.FileDescriptorLength - fileNameBytes.Length - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write(model.CreatedOn);
            writer.Write(model.UpdatedOn);
            writer.Write(model.DataOffset);
            writer.Write(model.DataLength);

            _logger.Information("Done writing file descriptor data");

            return buffer;
        }
    }
}