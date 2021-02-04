using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    public class FilesystemDescriptorRepository : IFilesystemDescriptorRepository
    {
        private readonly IStorageConnection _storageConnection;
        private readonly ISerializer<FilesystemDescriptor> _serializer;

        public FilesystemDescriptorRepository(IStorageConnection storageConnection, ISerializer<FilesystemDescriptor> serializer)
        {
            _storageConnection = storageConnection;
            _serializer = serializer;
        }

        public FilesystemDescriptor Read()
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var length = FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _storageConnection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            return descriptor;
        }

        public void Write(FilesystemDescriptor model)
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(model);

            _storageConnection.PerformWrite(new Cursor(offset, origin), data);
        }
    }
}