using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess
{
    public class FileFsInitializer : IFileFsInitializer
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;

        public FileFsInitializer(ISerializer<FilesystemDescriptor> serializer)
        {
            _serializer = serializer;
        }

        public void Initialize(string fileName, int fileSize, int fileNameLength, int version)
        {
            var fileSystemDescriptor =
                new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename, version);
            var buffer = _serializer.ToBuffer(fileSystemDescriptor);

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            stream.Write(buffer);
        }
    }
}