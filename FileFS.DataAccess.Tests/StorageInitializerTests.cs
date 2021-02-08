// Missing XML comment for publicly visible type or member...

using System;
using System.IO;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests
{
    public class StorageInitializerTests
    {
        [Theory]
        [InlineData(1000, 10)]
        [InlineData(100, 1)]
        [InlineData(100000, 1289)]
        public void Initialize_ShouldCreateStorageOfSizeAndFileNameLength(int size, int fileNameLength)
        {
            // Arrange
            var logger = new LoggerConfiguration().CreateLogger();
            using var storageStream = new MemoryStream(size);
            var storageStreamProvider = StorageStreamProviderMockFactory.Create(storageStream);
            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var storageInitializer = new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);
            var expectedFilesystemDescriptor =
                new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename);

            // Act
            storageInitializer.Initialize(size, fileNameLength);

            // Arrange
            using var newStorageStream = new MemoryStream(storageStream.GetBuffer());
            newStorageStream.Seek(size - FilesystemDescriptor.BytesTotal, SeekOrigin.Begin);
            var filesystemDescriptorBuffer = new byte[FilesystemDescriptor.BytesTotal];
            newStorageStream.Read(filesystemDescriptorBuffer);
            var filesystemDescriptor = filesystemDescriptorSerializer.FromBuffer(filesystemDescriptorBuffer);
            Assert.Equal(size, newStorageStream.Length);
            Assert.Equal(expectedFilesystemDescriptor, filesystemDescriptor);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(100, 0)]
        [InlineData(0, 0)]
        [InlineData(0, -10)]
        [InlineData(-10, 0)]
        [InlineData(-10, -10)]
        public void Initialize_ShouldFailSizeOrFilenameLengthValidation(int size, int fileNameLength)
        {
            // Arrange
            var logger = new LoggerConfiguration().CreateLogger();
            using var storageStream = new MemoryStream();
            var storageStreamProvider = StorageStreamProviderMockFactory.Create(storageStream);
            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var storageInitializer = new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);

            // Act
            void Act() => storageInitializer.Initialize(size, fileNameLength);

            // Arrange
            Assert.Throws<ArgumentException>(Act);
        }
    }
}