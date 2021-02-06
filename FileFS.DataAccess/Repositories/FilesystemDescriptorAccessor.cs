using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers.Abstractions;

namespace FileFS.DataAccess.Repositories
{
    public class FilesystemDescriptorAccessor : IFilesystemDescriptorAccessor
    {
        private readonly IStorageConnection _storageConnection;
        private readonly ISerializer<FilesystemDescriptor> _serializer;

        public FilesystemDescriptorAccessor(IStorageConnection storageConnection, ISerializer<FilesystemDescriptor> serializer)
        {
            _storageConnection = storageConnection;
            _serializer = serializer;
        }

        public FilesystemDescriptor Value => Read();

        public void Update(FilesystemDescriptor descriptor)
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(descriptor);

            _storageConnection.PerformWrite(new Cursor(offset, origin), data);
        }

        private FilesystemDescriptor Read()
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var length = FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _storageConnection.PerformRead(new Cursor(offset, origin), length);
            var descriptor = _serializer.FromBuffer(data);

            return descriptor;
        }
    }
}