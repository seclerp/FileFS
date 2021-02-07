using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Extensions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// Stateless storage connection implementation.
    /// </summary>
    public class StorageConnection : IStorageConnection
    {
        private readonly string _fileFsStoragePath;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageConnection"/> class.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to a existing file that is used as FileFS storage.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageConnection(string fileFsStoragePath, ILogger logger)
        {
            _fileFsStoragePath = fileFsStoragePath;
            _logger = logger;
        }

        /// <inheritdoc />
        public void PerformWrite(Cursor cursor, byte[] data)
        {
            _logger.Information($"Start write operation at offset {cursor.Offset} for {data.Length} bytes");

            using var stream = OpenStream();
            using var writer = CreateWriter(stream);

            stream.Seek(cursor);
            writer.Write(data);

            _logger.Information($"Done write operation at offset {cursor.Offset} for {data.Length} bytes");
        }

        /// <inheritdoc />
        public byte[] PerformRead(Cursor cursor, int length)
        {
            _logger.Information($"Start read operation at offset {cursor.Offset} for {length} bytes");

            using var stream = OpenStream();
            using var reader = CreateReader(stream);

            stream.Seek(cursor);
            var bytes = reader.ReadBytes(length);

            _logger.Information($"Done read operation at offset {cursor.Offset} for {length} bytes");

            return bytes;
        }

        /// <inheritdoc />
        public void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length)
        {
            _logger.Information($"Start copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");

            using var tempStream = new MemoryStream(length);
            using var tempStreamReader = CreateReader(tempStream);
            using var tempStreamWriter = CreateWriter(tempStream);

            using var stream = OpenStream();
            using var reader = CreateReader(stream);
            using var writer = CreateWriter(stream);

            stream.Seek(sourceCursor);
            tempStreamWriter.Write(reader.ReadBytes(length));
            stream.Seek(destinationCursor);
            tempStream.Seek(0, SeekOrigin.Begin);
            writer.Write(tempStreamReader.ReadBytes(length));

            _logger.Information($"Done copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");
        }

        /// <inheritdoc />
        public long GetSize()
        {
            using var stream = OpenStream();
            return stream.Length;
        }

        /// <inheritdoc />
        public Stream OpenStream()
        {
            _logger.Information($"Trying to open stream for filename {_fileFsStoragePath}");

            var stream = new FileStream(_fileFsStoragePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            _logger.Information($"Stream for filename {_fileFsStoragePath} opened");

            return stream;
        }

        private static BinaryReader CreateReader(Stream stream)
        {
            return new BinaryReader(stream);
        }

        private static BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }
    }
}