using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess.Repositories
{
    /// <summary>
    /// Filesystem descriptor access implementation.
    /// </summary>
    public class FilesystemDescriptorAccessor : IFilesystemDescriptorAccessor
    {
        private readonly IStorageConnection _connection;
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemDescriptorAccessor"/> class.
        /// </summary>
        /// <param name="connection">Storage connection instance.</param>
        /// <param name="serializer">Filesystem descriptor Serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public FilesystemDescriptorAccessor(
            IStorageConnection connection,
            ISerializer<FilesystemDescriptor> serializer,
            ILogger logger)
        {
            _connection = connection;
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public FilesystemDescriptor Value => Read();

        /// <inheritdoc />
        public void Update(FilesystemDescriptor descriptor)
        {
            _logger.Information("Trying to update filesystem descriptor");

            var offset = -FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(descriptor);

            _connection.PerformWrite(new Cursor(offset, origin), data);

            _logger.Information("Filesystem descriptor updated");
        }

        private FilesystemDescriptor Read()
        {
            _logger.Information("Trying to retrieve filesystem descriptor");

            const SeekOrigin origin = SeekOrigin.End;

            var offset = -FilesystemDescriptor.BytesTotal;
            var length = FilesystemDescriptor.BytesTotal;
            var data = _connection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            _logger.Information("Filesystem descriptor retrieved");

            return descriptor;
        }
    }
}