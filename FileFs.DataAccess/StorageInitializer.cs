using System.IO;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Serializers.Abstractions;
using Microsoft.Extensions.Logging;

namespace FileFs.DataAccess
{
    public class StorageInitializer : IStorageInitializer
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly ILogger<StorageInitializer> _logger;

        public StorageInitializer(ISerializer<FilesystemDescriptor> serializer, ILogger<StorageInitializer> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public void Initialize(string fileName, int fileSize, int fileNameLength, int version)
        {
            _logger.LogInformation($"Start storage initialization process, filename {fileName}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes, version {version}");

            var fileSystemDescriptor =
                new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename, version);
            var buffer = _serializer.ToBuffer(fileSystemDescriptor);

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            stream.Write(buffer);

            _logger.LogInformation($"Done storage initialization process, filename {fileName}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes, version {version}");
        }
    }
}