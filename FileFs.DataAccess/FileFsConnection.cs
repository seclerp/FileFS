using System.IO;
using FileFs.DataAccess.Abstractions;

namespace FileFs.DataAccess
{
    public class FileFsConnection : IFileFsConnection
    {
        private readonly string _fileName;

        public FileFsConnection(string fileName)
        {
            _fileName = fileName;
        }

        public void PerformWrite(int offset, byte[] data, SeekOrigin origin)
        {
            using var stream = OpenStream();
            using var writer = CreateWriter(stream);

            writer.Seek(offset, origin);
            writer.Write(data);
        }

        public byte[] PerformRead(int offset, int length, SeekOrigin origin)
        {
            using var stream = OpenStream();
            using var reader = CreateReader(stream);

            stream.Seek(offset, origin);
            var bytes = reader.ReadBytes(length);

            return bytes;
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