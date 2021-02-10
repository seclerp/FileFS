using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers;
using Moq;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Serializers
{
    public class FileDescriptorSerializerTests
    {
        [Theory]
        [InlineData(35, "foo", 100, 100, 0, 0)]
        [InlineData(100, "bar", 0, 0, 12312, 12)]
        public void ToBuffer_ShouldCreatedCorrectBufferWithData(int fileDescriptorLength, string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var fileDescriptor = new FileDescriptor(fileName, createdOn, updatedOn, dataOffset, dataLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var filesystemDescriptorAccessor = CreateFilesystemDescriptorAccessor(filesystemDescriptor);
            var serializer = new FileDescriptorSerializer(filesystemDescriptorAccessor, logger);

            // Act
            var buffer = serializer.ToBytes(fileDescriptor);

            // Assert
            using var memoryStream = new MemoryStream(buffer);
            using var reader = new BinaryReader(memoryStream);

            var writtenStringLength = reader.ReadInt32();
            var writtenFileNameBytes = reader.ReadBytes(writtenStringLength);
            var writtenFileName = Encoding.UTF8.GetString(writtenFileNameBytes);
            memoryStream.Seek(filesystemDescriptor.FileDescriptorLength - writtenStringLength - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var writtenCreatedOn = reader.ReadInt64();
            var writtenUpdatedOn = reader.ReadInt64();
            var writtenDataOffset = reader.ReadInt32();
            var writtenDataLength = reader.ReadInt32();

            Assert.Equal(fileDescriptorLength, buffer.Length);
            Assert.Equal(fileName, writtenFileName);
            Assert.Equal(createdOn, writtenCreatedOn);
            Assert.Equal(updatedOn, writtenUpdatedOn);
            Assert.Equal(dataOffset, writtenDataOffset);
            Assert.Equal(dataLength, writtenDataLength);
        }

        [Theory]
        [InlineData(35, "foo", 100, 100, 0, 0)]
        [InlineData(100, "bar", 0, 0, 12312, 12)]
        public void FromBuffer_ShouldCreatedCorrectBufferWithData(int fileDescriptorLength, string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var filesystemDescriptorAccessor = CreateFilesystemDescriptorAccessor(filesystemDescriptor);
            var serializer = new FileDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var buffer = new byte[fileDescriptorLength];

            using var memoryStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(memoryStream);

            var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            writer.Write(fileName.Length);
            writer.Write(fileNameBytes);
            writer.Seek(filesystemDescriptor.FileDescriptorLength - fileNameBytes.Length - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write(createdOn);
            writer.Write(updatedOn);
            writer.Write(dataOffset);
            writer.Write(dataLength);

            // Act
            var fileDescriptor = serializer.FromBytes(buffer);

            // Assert
            Assert.Equal(fileDescriptor.FileNameLength, fileNameBytes.Length);
            Assert.Equal(fileDescriptor.FileName, fileName);
            Assert.Equal(fileDescriptor.CreatedOn, createdOn);
            Assert.Equal(fileDescriptor.UpdatedOn, updatedOn);
            Assert.Equal(fileDescriptor.DataOffset, dataOffset);
            Assert.Equal(fileDescriptor.DataLength, dataLength);
        }

        private static IFilesystemDescriptorAccessor CreateFilesystemDescriptorAccessor(FilesystemDescriptor stub)
        {
            var filesystemDescriptorAccessorMock = new Mock<IFilesystemDescriptorAccessor>();
            filesystemDescriptorAccessorMock.Setup(serializer => serializer.Value).Returns(stub);

            return filesystemDescriptorAccessorMock.Object;
        }
    }
}