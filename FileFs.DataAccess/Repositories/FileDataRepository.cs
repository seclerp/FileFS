using System.IO;
using FileFs.DataAccess.Repositories.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    public class FileDataRepository : IFileDataRepository
    {
        private readonly StorageConnection _storageConnection;

        public FileDataRepository(StorageConnection storageConnection)
        {
            _storageConnection = storageConnection;
        }

        public byte[] Read(int offset, int length)
        {
            var origin = SeekOrigin.Begin;

            return _storageConnection.PerformRead(new Cursor(offset, origin), length);
        }

        public void Write(byte[] data, int offset)
        {
            var origin = SeekOrigin.Begin;

            _storageConnection.PerformWrite(new Cursor(offset, origin), data);
        }

        public void Copy(int sourceOffset, int destinationOffset, int length)
        {
            var origin = SeekOrigin.Begin;

            _storageConnection.PerformCopy(new Cursor(sourceOffset, origin), new Cursor(destinationOffset, origin), length);
        }
    }
}