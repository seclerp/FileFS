using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    public class FilesystemDescriptorRepository : IFilesystemDescriptorRepository
    {
        private readonly IFileFsConnection _connection;
        private readonly ISerializer<FilesystemDescriptor> _serializer;

        public FilesystemDescriptorRepository(IFileFsConnection connection, ISerializer<FilesystemDescriptor> serializer)
        {
            _connection = connection;
            _serializer = serializer;
        }

        public FilesystemDescriptor Read()
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var length = FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _connection.PerformRead(offset, length, origin);
            var descriptor = _serializer.FromBuffer(data);

            return descriptor;
        }

        public void Write(FilesystemDescriptor model)
        {
            var offset = -FilesystemDescriptor.BytesTotal;
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(model);

            _connection.PerformWrite(offset, data, origin);
        }
    }
}