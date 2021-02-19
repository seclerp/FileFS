using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Extensions;
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
    public class EntryDescriptorSerializerTests
    {
        [Theory]
        [InlineData(48, "foo", 100, 100, 0, 0)]
        [InlineData(100, "bar", 0, 0, 12312, 12)]
        public void ToBuffer_ShouldCreatedCorrectBufferWithData(int fileDescriptorLength, string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var id = Guid.NewGuid();
            var type = EntryType.File;
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var fileDescriptor = new EntryDescriptor(id, fileName, EntryType.File, createdOn, updatedOn, dataOffset, dataLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var filesystemDescriptorAccessor = CreateFilesystemDescriptorAccessor(filesystemDescriptor);
            var serializer = new EntryDescriptorSerializer(filesystemDescriptorAccessor, logger);

            // Act
            var buffer = serializer.ToBytes(fileDescriptor);

            // Assert
            using var memoryStream = new MemoryStream(buffer);
            using var reader = new BinaryReader(memoryStream);

            var writtenIdBytes = reader.ReadGuidBytes();
            var writtenId = new Guid(writtenIdBytes);
            var writtenStringLength = reader.ReadInt32();
            var writtenFileNameBytes = reader.ReadBytes(writtenStringLength);
            var writtenFileName = Encoding.UTF8.GetString(writtenFileNameBytes);
            memoryStream.Seek(filesystemDescriptor.EntryDescriptorLength - writtenStringLength - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var writtenType = (EntryType)reader.ReadByte();
            var writtenCreatedOn = reader.ReadInt64();
            var writtenUpdatedOn = reader.ReadInt64();
            var writtenDataOffset = reader.ReadInt32();
            var writtenDataLength = reader.ReadInt32();

            Assert.Equal(id, writtenId);
            Assert.Equal(fileDescriptorLength, buffer.Length);
            Assert.Equal(fileName, writtenFileName);
            Assert.Equal(writtenType, type);
            Assert.Equal(createdOn, writtenCreatedOn);
            Assert.Equal(updatedOn, writtenUpdatedOn);
            Assert.Equal(dataOffset, writtenDataOffset);
            Assert.Equal(dataLength, writtenDataLength);
        }

        [Theory]
        [InlineData(48, "foo", 100, 100, 0, 0)]
        [InlineData(100, "bar", 0, 0, 12312, 12)]
        public void FromBuffer_ShouldCreatedCorrectBufferWithData(int fileDescriptorLength, string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var filesystemDescriptorAccessor = CreateFilesystemDescriptorAccessor(filesystemDescriptor);
            var serializer = new EntryDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var buffer = new byte[fileDescriptorLength];
            var id = Guid.NewGuid();
            var type = EntryType.File;

            using var memoryStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(memoryStream);

            writer.Write(id.ToByteArray());
            var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            writer.Write(fileName.Length);
            writer.Write(fileNameBytes);
            writer.Seek(filesystemDescriptor.EntryDescriptorLength - fileNameBytes.Length - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write((byte)type);
            writer.Write(createdOn);
            writer.Write(updatedOn);
            writer.Write(dataOffset);
            writer.Write(dataLength);

            // Act
            var fileDescriptor = serializer.FromBytes(buffer);

            // Assert
            Assert.Equal(fileNameBytes.Length, fileDescriptor.EntryNameLength);
            Assert.Equal(fileName, fileDescriptor.EntryName);
            Assert.Equal(createdOn, fileDescriptor.CreatedOn);
            Assert.Equal(updatedOn, fileDescriptor.UpdatedOn);
            Assert.Equal(dataOffset, fileDescriptor.DataOffset);
            Assert.Equal(dataLength, fileDescriptor.DataLength);
        }

        private static IFilesystemDescriptorAccessor CreateFilesystemDescriptorAccessor(FilesystemDescriptor stub)
        {
            var filesystemDescriptorAccessorMock = new Mock<IFilesystemDescriptorAccessor>();
            filesystemDescriptorAccessorMock.Setup(serializer => serializer.Value).Returns(stub);

            return filesystemDescriptorAccessorMock.Object;
        }
    }
}