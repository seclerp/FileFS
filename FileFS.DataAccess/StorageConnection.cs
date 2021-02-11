using System;
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
        private readonly IStorageStreamProvider _storageStreamProvider;
        private readonly ILogger _logger;
        private readonly int _bufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageConnection"/> class.
        /// </summary>
        /// <param name="storageStreamProvider">Storage stream provider instance.</param>
        /// <param name="bufferSize">Buffer size to use in buffered operations.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageConnection(IStorageStreamProvider storageStreamProvider, int bufferSize, ILogger logger)
        {
            _storageStreamProvider = storageStreamProvider;
            _logger = logger;
            _bufferSize = bufferSize;
        }

        /// <inheritdoc />
        public void PerformWrite(Cursor cursor, byte[] data)
        {
            _logger.Information($"Start write operation at offset {cursor.Offset} for {data.Length} bytes");

            using var stream = _storageStreamProvider.OpenStream();
            using var writer = CreateWriter(stream);

            stream.Seek(cursor);
            writer.Write(data);

            _logger.Information($"Done write operation at offset {cursor.Offset} for {data.Length} bytes");
        }

        /// <inheritdoc />
        public void PerformWrite(Cursor cursor, int length, Stream sourceStream)
        {
            _logger.Information($"Start buffered write operation at offset {cursor.Offset} for {length} bytes");

            using var stream = _storageStreamProvider.OpenStream();

            stream.Seek(cursor);
            sourceStream.WriteBuffered(stream, length, _bufferSize);

            _logger.Information($"Done buffered write operation at offset {cursor.Offset} for {length} bytes");
        }

        /// <inheritdoc />
        public byte[] PerformRead(Cursor cursor, int length)
        {
            _logger.Information($"Start read operation at offset {cursor.Offset} for {length} bytes");

            var bytes = Array.Empty<byte>();

            // Do not read anything if size is zero, not necessary
            if (length > 0)
            {
                using var stream = _storageStreamProvider.OpenStream();
                using var reader = CreateReader(stream);

                stream.Seek(cursor);
                bytes = reader.ReadBytes(length);
            }

            _logger.Information($"Done read operation at offset {cursor.Offset} for {length} bytes");

            return bytes;
        }

        /// <inheritdoc />
        public void PerformRead(Cursor cursor, int length, Stream destinationStream)
        {
            _logger.Information($"Start buffered read operation at offset {cursor.Offset} for {length} bytes");

            // Do not read anything if size is zero, not necessary
            if (length > 0)
            {
                using var stream = _storageStreamProvider.OpenStream();

                stream.Seek(cursor);
                stream.WriteBuffered(destinationStream, length, _bufferSize);
            }

            _logger.Information($"Done buffered read operation at offset {cursor.Offset} for {length} bytes");
        }

        /// <inheritdoc />
        public void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length)
        {
            _logger.Information($"Start copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");

            // Do not copy anything if size is zero, not necessary
            if (length > 0)
            {
                using var tempStream = new MemoryStream(length);
                using var tempStreamReader = CreateReader(tempStream);
                using var tempStreamWriter = CreateWriter(tempStream);

                using var stream = _storageStreamProvider.OpenStream();
                using var reader = CreateReader(stream);
                using var writer = CreateWriter(stream);

                stream.Seek(sourceCursor);
                tempStreamWriter.Write(reader.ReadBytes(length));
                stream.Seek(destinationCursor);
                tempStream.Seek(0, SeekOrigin.Begin);
                writer.Write(tempStreamReader.ReadBytes(length));
            }

            _logger.Information($"Done copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");
        }

        /// <inheritdoc />
        public long GetSize()
        {
            using var stream = _storageStreamProvider.OpenStream();
            return stream.Length;
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