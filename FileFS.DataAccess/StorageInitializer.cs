using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// FileStream based storage initializer implementation.
    /// </summary>
    public class StorageInitializer : IStorageInitializer
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly IStorageStreamProvider _storageStreamProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageInitializer"/> class.
        /// </summary>
        /// <param name="storageStreamProvider">Storage stream provider instance.</param>
        /// <param name="serializer">Filesystem descriptor serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageInitializer(IStorageStreamProvider storageStreamProvider, ISerializer<FilesystemDescriptor> serializer, ILogger logger)
        {
            _serializer = serializer;
            _storageStreamProvider = storageStreamProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNonValidException">Throws when fileSize less than reserved bytes for filesystem descriptor.</exception>
        /// <exception cref="ArgumentNonValidException">Throws when fileNameLength less or equals to 0.</exception>
        public void Initialize(int fileSize, int fileNameLength)
        {
            if (fileSize <= 0)
            {
                throw new ArgumentNonValidException($"Value '{nameof(fileSize)}' cannot be less than reserved bytes for filesystem descriptor ({FilesystemDescriptor.BytesTotal})");
            }

            if (fileNameLength <= 0)
            {
                throw new ArgumentNonValidException($"Value '{nameof(fileNameLength)}'cannot be less or equals to 0");
            }

            _logger.Information($"Start storage initialization process, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");

            var fileSystemDescriptor = new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename);
            var buffer = _serializer.ToBytes(fileSystemDescriptor);

            using var stream = _storageStreamProvider.OpenStream(false);
            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            stream.Write(buffer);

            _logger.Information($"Done storage initialization process, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");
        }
    }
}