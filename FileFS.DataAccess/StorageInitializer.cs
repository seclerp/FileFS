﻿using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers.Abstractions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// FileStream based storage initializer implementation.
    /// </summary>
    public class StorageInitializer : IStorageInitializer
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageInitializer"/> class.
        /// </summary>
        /// <param name="serializer">Filesystem descriptor serializer instance.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageInitializer(ISerializer<FilesystemDescriptor> serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Initialize(string fileFsStoragePath, int fileSize, int fileNameLength)
        {
            _logger.Information($"Start storage initialization process, filename {fileFsStoragePath}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");

            var fileSystemDescriptor = new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename);
            var buffer = _serializer.ToBuffer(fileSystemDescriptor);

            using var stream = new FileStream(fileFsStoragePath, FileMode.Create, FileAccess.Write);
            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            stream.Write(buffer);

            _logger.Information($"Done storage initialization process, filename {fileFsStoragePath}, storage size {fileSize} bytes, max file name length {fileNameLength} bytes");
        }
    }
}