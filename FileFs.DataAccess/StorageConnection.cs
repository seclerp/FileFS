using System.IO;
using FileFs.DataAccess.Abstractions;
using Microsoft.Extensions.Logging;

namespace FileFs.DataAccess
{
    public class StorageConnection : IStorageConnection
    {
        private readonly string _fileName;
        private readonly ILogger<StorageConnection> _logger;

        public StorageConnection(string fileName, ILogger<StorageConnection> logger)
        {
            _fileName = fileName;
            _logger = logger;
        }

        public void PerformWrite(Cursor cursor, byte[] data)
        {
            _logger.LogInformation($"Start write operation at offset {cursor.Offset} for {data.Length} bytes");

            using var stream = OpenStream();
            using var writer = CreateWriter(stream);

            writer.Seek(cursor.Offset, cursor.Origin);
            writer.Write(data);

            _logger.LogInformation($"Done write operation at offset {cursor.Offset} for {data.Length} bytes");
        }

        public byte[] PerformRead(Cursor cursor, int length)
        {
            _logger.LogInformation($"Start read operation at offset {cursor.Offset} for {length} bytes");

            using var stream = OpenStream();
            using var reader = CreateReader(stream);

            stream.Seek(cursor.Offset, cursor.Origin);
            var bytes = reader.ReadBytes(length);

            _logger.LogInformation($"Done read operation at offset {cursor.Offset} for {length} bytes");

            return bytes;
        }

        public void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length)
        {
            _logger.LogInformation($"Start copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");

            using var tempStream = new MemoryStream(length);
            using var tempStreamReader = CreateReader(tempStream);
            using var tempStreamWriter = CreateWriter(tempStream);

            using var stream = OpenStream();
            using var reader = CreateReader(stream);
            using var writer = CreateWriter(stream);

            stream.Seek(sourceCursor.Offset, sourceCursor.Origin);
            tempStreamWriter.Write(reader.ReadBytes(length));
            stream.Seek(destinationCursor.Offset, destinationCursor.Origin);
            tempStream.Seek(0, SeekOrigin.Begin);
            writer.Write(tempStreamReader.ReadBytes(length));

            _logger.LogInformation($"Done copy operation from offset {sourceCursor.Offset} to offset {destinationCursor.Offset}, {length} bytes length");
        }

        public long GetSize()
        {
            using var stream = OpenStream();
            return stream.Length;
        }

        public Stream OpenStream()
        {
            _logger.LogInformation($"Trying to open stream for filename {_fileName}");

            var stream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            _logger.LogInformation($"Stream for filename {_fileName} opened");

            return stream;
        }

        private BinaryReader CreateReader(Stream stream)
        {
            return new BinaryReader(stream);
        }

        private BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }
    }
}