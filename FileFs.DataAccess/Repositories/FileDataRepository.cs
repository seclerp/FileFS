using System.IO;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    class FileDataRepository : IFileDataRepository
    {
        private readonly FileFsConnection _connection;

        public FileDataRepository(FileFsConnection connection)
        {
            _connection = connection;
        }

        public byte[] Read(int offset, int length)
        {
            var origin = SeekOrigin.Begin;

            return _connection.PerformRead(offset, length, origin);
        }

        public void Write(byte[] data, int offset)
        {
            var origin = SeekOrigin.Begin;

            _connection.PerformWrite(offset, data, origin);
        }
    }
}