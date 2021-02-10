using System.IO;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Tests.Factories;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests
{
    public class FilesystemDescriptorAccessorTests
    {
        [Theory]
        [InlineData(1000, 100)]
        public void Value_ShouldReturnValidDescriptor(int bufferSize, int fileDescriptorLength)
        {
            // Arrange
            var buffer = new byte[bufferSize];
            var storageStreamProvider = StorageStreamProviderMockFactory.Create(buffer);
            var logger = new LoggerConfiguration().CreateLogger();
            var storageConnection = new StorageConnection(storageStreamProvider, logger, buffer.Length);
            var filesystemSerializer = new FilesystemDescriptorSerializer(logger);

            var expectedDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var filesystemDescriptorBytes = filesystemSerializer.ToBytes(expectedDescriptor);
            storageConnection.PerformWrite(new Cursor(-FilesystemDescriptor.BytesTotal, SeekOrigin.End), filesystemDescriptorBytes);

            var filesystemDescriptorAccessor =
                new FilesystemDescriptorAccessor(storageConnection, filesystemSerializer, logger);

            // Act
            var returnedDescriptor = filesystemDescriptorAccessor.Value;

            // Assert
            Assert.Equal(expectedDescriptor, returnedDescriptor);
        }

        [Theory]
        [InlineData(1000, 100)]
        public void Update_ShouldReturnUpdatedDescriptor(int bufferSize, int fileDescriptorLength)
        {
            // Arrange
            var buffer = new byte[bufferSize];
            var storageStreamProvider = StorageStreamProviderMockFactory.Create(buffer);
            var logger = new LoggerConfiguration().CreateLogger();
            var storageConnection = new StorageConnection(storageStreamProvider, logger, buffer.Length);
            var filesystemSerializer = new FilesystemDescriptorSerializer(logger);

            var expectedDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var filesystemDescriptorBytes = filesystemSerializer.ToBytes(expectedDescriptor);
            var filesystemDescriptorCursor = new Cursor(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            storageConnection.PerformWrite(filesystemDescriptorCursor, filesystemDescriptorBytes);

            var filesystemDescriptorAccessor =
                new FilesystemDescriptorAccessor(storageConnection, filesystemSerializer, logger);

            // Act
            filesystemDescriptorAccessor.Update(expectedDescriptor);

            // Assert
            var returnedDescriptorBytes =
                storageConnection.PerformRead(filesystemDescriptorCursor, FilesystemDescriptor.BytesTotal);
            var returnedDescriptor = filesystemSerializer.FromBytes(returnedDescriptorBytes);
            Assert.Equal(expectedDescriptor, returnedDescriptor);
        }
    }
}