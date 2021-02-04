using System.IO;
using FileFs.DataAccess.Abstractions;

namespace FileFs.DataAccess
{
    public class StorageConnection : IStorageConnection
    {
        private readonly string _fileName;

        public StorageConnection(string fileName)
        {
            _fileName = fileName;
        }

        public void PerformWrite(Cursor cursor, byte[] data)
        {
            using var stream = OpenStream();
            using var writer = CreateWriter(stream);

            writer.Seek(cursor.Offset, cursor.Origin);
            writer.Write(data);
        }

        public byte[] PerformRead(Cursor cursor, int length)
        {
            using var stream = OpenStream();
            using var reader = CreateReader(stream);

            stream.Seek(cursor.Offset, cursor.Origin);
            var bytes = reader.ReadBytes(length);

            return bytes;
        }

        public void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length)
        {
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
        }

        public long GetSize()
        {
            using var stream = OpenStream();
            return stream.Length;
        }

        private Stream OpenStream()
        {
            return new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
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