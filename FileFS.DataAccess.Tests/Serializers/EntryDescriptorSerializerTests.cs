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
        [InlineData(65, "/foo", 100, 100, 0, 0, EntryType.File)]
        [InlineData(100, "/bar", 0, 0, 12312, 12, EntryType.Directory)]
        public void ToBuffer_ShouldCreatedCorrectBufferWithData(int fileDescriptorLength, string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength, EntryType type)
        {
            // Arrange
            var id = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var fileDescriptor = new EntryDescriptor(id, parentId, fileName, type, createdOn, updatedOn, dataOffset, dataLength);
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
            var writtenParentIdBytes = reader.ReadGuidBytes();
            var writtenParentId = new Guid(writtenParentIdBytes);
            var writtenStringLength = reader.ReadInt32();
            var writtenEntryNameBytes = reader.ReadBytes(writtenStringLength);
            var writtenEntryName = Encoding.UTF8.GetString(writtenEntryNameBytes);
            memoryStream.Seek(filesystemDescriptor.EntryDescriptorLength - writtenStringLength - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var writtenType = (EntryType)reader.ReadByte();
            var writtenCreatedOn = reader.ReadInt64();
            var writtenUpdatedOn = reader.ReadInt64();
            var writtenDataOffset = reader.ReadInt32();
            var writtenDataLength = reader.ReadInt32();

            Assert.Equal(id, writtenId);
            Assert.Equal(parentId, writtenParentId);
            Assert.Equal(fileDescriptorLength, buffer.Length);
            Assert.Equal(fileName, writtenEntryName);
            Assert.Equal(type, writtenType);
            Assert.Equal(createdOn, writtenCreatedOn);
            Assert.Equal(updatedOn, writtenUpdatedOn);
            Assert.Equal(dataOffset, writtenDataOffset);
            Assert.Equal(dataLength, writtenDataLength);
        }

        [Theory]
        [InlineData(65, "/foo", 100, 100, 0, 0, EntryType.File)]
        [InlineData(100, "/bar", 0, 0, 12312, 12, EntryType.Directory)]
        public void FromBuffer_ShouldCreatedCorrectModelFromBuffer(int fileDescriptorLength, string entryName, long createdOn, long updatedOn, int dataOffset, int dataLength, EntryType type)
        {
            // Arrange
            var filesystemDescriptor = new FilesystemDescriptor(0, 0, fileDescriptorLength);
            var logger = new LoggerConfiguration().CreateLogger();
            var filesystemDescriptorAccessor = CreateFilesystemDescriptorAccessor(filesystemDescriptor);
            var serializer = new EntryDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var buffer = new byte[fileDescriptorLength];
            var id = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            using var memoryStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(memoryStream);

            writer.Write(id.ToByteArray());
            writer.Write(parentId.ToByteArray());
            var entryNameBytes = Encoding.UTF8.GetBytes(entryName);
            writer.Write(entryName.Length);
            writer.Write(entryNameBytes);
            writer.Seek(filesystemDescriptor.EntryDescriptorLength - entryNameBytes.Length - EntryDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write((byte)type);
            writer.Write(createdOn);
            writer.Write(updatedOn);
            writer.Write(dataOffset);
            writer.Write(dataLength);

            // Act
            var entryDescriptor = serializer.FromBytes(buffer);

            // Assert
            Assert.Equal(id, entryDescriptor.Id);
            Assert.Equal(parentId, entryDescriptor.ParentId);
            Assert.Equal(entryNameBytes.Length, entryDescriptor.NameLength);
            Assert.Equal(entryNameBytes.Length, entryDescriptor.NameLength);
            Assert.Equal(entryName, entryDescriptor.Name);
            Assert.Equal(type, entryDescriptor.Type);
            Assert.Equal(createdOn, entryDescriptor.CreatedOn);
            Assert.Equal(updatedOn, entryDescriptor.UpdatedOn);
            Assert.Equal(dataOffset, entryDescriptor.DataOffset);
            Assert.Equal(dataLength, entryDescriptor.DataLength);
        }

        private static IFilesystemDescriptorAccessor CreateFilesystemDescriptorAccessor(FilesystemDescriptor stub)
        {
            var filesystemDescriptorAccessorMock = new Mock<IFilesystemDescriptorAccessor>();
            filesystemDescriptorAccessorMock.Setup(serializer => serializer.Value).Returns(stub);

            return filesystemDescriptorAccessorMock.Object;
        }
    }
}