using System.Collections.Generic;
using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess.Repositories
{
    public class FileDescriptorRepository : IFileDescriptorRepository
    {
        private readonly IFileFsConnection _connection;
        private readonly IFilesystemDescriptorRepository _filesystemDescriptorRepository;
        private readonly ISerializer<FileDescriptor> _serializer;

        public FileDescriptorRepository(
            IFileFsConnection connection,
            IFilesystemDescriptorRepository filesystemDescriptorRepository,
            ISerializer<FileDescriptor> serializer)
        {
            _connection = connection;
            _filesystemDescriptorRepository = filesystemDescriptorRepository;
            _serializer = serializer;
        }

        public FileDescriptor Read(int offset)
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();
            var length = filesystemDescriptor.FileDescriptorLength;
            var origin = SeekOrigin.End;
            var data = _connection.PerformRead(offset, length, origin);
            var descriptor = _serializer.FromBuffer(data);

            return descriptor;
        }

        public IReadOnlyCollection<FileDescriptor> ReadAll()
        {
            var filesystemDescriptor = _filesystemDescriptorRepository.Read();

            var startFromOffset = -FilesystemDescriptor.BytesTotal - filesystemDescriptor.FileDescriptorLength;
            var endOffset = -FilesystemDescriptor.BytesTotal -
                            (filesystemDescriptor.FileDescriptorsCount *
                             filesystemDescriptor.FileDescriptorLength);

            var allDescriptors = new FileDescriptor[filesystemDescriptor.FileDescriptorsCount];
            var index = 0;
            for (var offset = startFromOffset; offset >= endOffset; offset -= filesystemDescriptor.FileDescriptorLength)
            {
                var length = filesystemDescriptor.FileDescriptorLength;
                var origin = SeekOrigin.End;
                var data = _connection.PerformRead(offset, length, origin);
                var descriptor = _serializer.FromBuffer(data);

                allDescriptors[index] = descriptor;
                index++;
            }

            return allDescriptors;
        }

        public void Write(FileDescriptor model, int offset)
        {
            var origin = SeekOrigin.End;
            var data = _serializer.ToBuffer(model);

            _connection.PerformWrite(offset, data, origin);
        }
    }
}