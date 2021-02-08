// Missing XML comment for publicly visible type or member...

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
    public class FilesystemDescriptorSerializerTests
    {
        [Theory]
        [InlineData(0, 0, 100)]
        [InlineData(25, 123, 10)]
        public void ToBuffer_ShouldCreatedCorrectBufferWithData(int filesDataLength, int fileDescriptorsCount, int fileDescriptorLength)
        {
            // Arrange
            var filesystemDescriptor =
                new FilesystemDescriptor(filesDataLength, fileDescriptorsCount, fileDescriptorLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var serializer = new FilesystemDescriptorSerializer(logger);

            // Act
            var buffer = serializer.ToBuffer(filesystemDescriptor);

            // Assert
            using var memoryStream = new MemoryStream(buffer);
            using var reader = new BinaryReader(memoryStream);

            var writtenFilesDataLength = reader.ReadInt32();
            var writtenFileDescriptorsCount = reader.ReadInt32();
            var writtenFileDescriptorLength = reader.ReadInt32();

            Assert.Equal(FilesystemDescriptor.BytesTotal, buffer.Length);
            Assert.Equal(filesDataLength, writtenFilesDataLength);
            Assert.Equal(fileDescriptorsCount, writtenFileDescriptorsCount);
            Assert.Equal(fileDescriptorLength, writtenFileDescriptorLength);
        }

        [Theory]
        [InlineData(0, 0, 100)]
        [InlineData(25, 123, 10)]
        public void FromBuffer_ShouldCreatedCorrectBufferWithData(int filesDataLength, int fileDescriptorsCount, int fileDescriptorLength)
        {
            // Arrange
            var logger = new LoggerConfiguration().CreateLogger();
            var serializer = new FilesystemDescriptorSerializer(logger);
            var buffer = new byte[FilesystemDescriptor.BytesTotal];

            using var memoryStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(memoryStream);

            writer.Write(filesDataLength);
            writer.Write(fileDescriptorsCount);
            writer.Write(fileDescriptorLength);

            // Act
            var filesystemDescriptor = serializer.FromBuffer(buffer);

            // Assert
            Assert.Equal(filesystemDescriptor.FilesDataLength, filesDataLength);
            Assert.Equal(filesystemDescriptor.FileDescriptorsCount, fileDescriptorsCount);
            Assert.Equal(filesystemDescriptor.FileDescriptorLength, fileDescriptorLength);
        }
    }
}