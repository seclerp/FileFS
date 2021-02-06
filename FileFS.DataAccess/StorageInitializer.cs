using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess
{
    public class StorageInitializer : IStorageInitializer
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly ILogger _logger;

        public StorageInitializer(ISerializer<FilesystemDescriptor> serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public void Initialize(string fileName, int fileSize, int fileNameLength)
        {
            _logger.Information($"Start storage initialization process, filename {fileName}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");

            var fileSystemDescriptor =
                new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename);
            var buffer = _serializer.ToBuffer(fileSystemDescriptor);

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            stream.Write(buffer);

            _logger.Information($"Done storage initialization process, filename {fileName}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");
        }
    }
}