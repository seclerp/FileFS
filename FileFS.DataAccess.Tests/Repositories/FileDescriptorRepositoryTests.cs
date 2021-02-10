using System.IO;
using System.Linq;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Tests.Factories;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Repositories
{
    public class FileDescriptorRepositoryTests
    {
        private const int FileNameLength = 100;

        [Theory]
        [InlineData("test.filename", 0, 0, 0, 0)]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("123123123", 123, 321, 123, 312)]
        public void Read_WhereThereIsOneDescriptor_ShouldReturnCorrectItem(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var fileDescriptor = new FileDescriptor(fileName, createdOn, updatedOn, dataOffset, dataLength);
            var repository = CreateRepository(storageBuffer, true);
            var cursor = new Cursor(-FilesystemDescriptor.BytesTotal - FileDescriptor.BytesWithoutFilename - FileNameLength, SeekOrigin.End);
            repository.Write(new StorageItem<FileDescriptor>(fileDescriptor, cursor));

            // Act
            var writtenFileDescriptor = repository.Read(cursor);

            // Assert
            Assert.Equal(fileDescriptor, writtenFileDescriptor.Value);
            Assert.Equal(cursor, writtenFileDescriptor.Cursor);
        }

        [Fact]
        public void ReadAll_WhereThereAreNoDescriptors_ShouldReturnEmptyCollection()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer, true);

            // Act
            var descriptors = repository.ReadAll();

            // Assert
            Assert.Empty(descriptors);
        }

        [Fact]
        public void ReadAll_WhenThereAreExistingItems_ShouldReturnCollectionWithAllItems()
        {
            // Arrange
            var storageBuffer = new byte[10000];

            var expectedDescriptors = new[]
            {
                new FileDescriptor("test1.storage", 0, 0, 0, 0),
                new FileDescriptor("test2.storage", 0, 0, 0, 0),
                new FileDescriptor("test3.storage", 0, 0, 0, 0),
                new FileDescriptor("test4.storage", 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, expectedDescriptors);

            // Act
            var writtenCollection = repository.ReadAll();

            // Assert
            Assert.Equal(expectedDescriptors, writtenCollection.Select(storageItem => storageItem.Value));
        }

        [Theory]
        [InlineData("test.filename", 0, 0, 0, 0)]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("123123123", 123, 321, 123, 312)]
        public void Write_StorageShouldContainCorrectItem(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var fileDescriptor = new FileDescriptor(fileName, createdOn, updatedOn, dataOffset, dataLength);
            var repository = CreateRepository(storageBuffer, true);
            var cursor = new Cursor(-FilesystemDescriptor.BytesTotal - FileDescriptor.BytesWithoutFilename - FileNameLength, SeekOrigin.End);

            // Act
            repository.Write(new StorageItem<FileDescriptor>(fileDescriptor, cursor));

            // Assert
            var writtenFileDescriptor = repository.Read(cursor);
            Assert.Equal(fileDescriptor, writtenFileDescriptor.Value);
            Assert.Equal(cursor, writtenFileDescriptor.Cursor);
        }

        [Fact]
        public void Find_ShouldReturnCorrectItemByFilename()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var expectedFoundDescriptor = new FileDescriptor("test3.storage", 0, 0, 0, 0);

            var descriptors = new[]
            {
                new FileDescriptor("test1.storage", 0, 0, 0, 0),
                new FileDescriptor("test2.storage", 0, 0, 0, 0),
                expectedFoundDescriptor,
                new FileDescriptor("test4.storage", 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var foundFileDescriptorItem = repository.Find(expectedFoundDescriptor.FileName);

            // Assert
            Assert.Equal(expectedFoundDescriptor, foundFileDescriptorItem.Value);
        }

        [Theory]
        [InlineData("test.filename", 0, 0, 0, 0)]
        public void Exists_WhenItemExists_ShouldReturnTrue(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var expectedFoundDescriptor = new FileDescriptor(fileName, createdOn, updatedOn, dataOffset, dataLength);

            var descriptors = new[]
            {
                new FileDescriptor("test1.storage", 0, 0, 0, 0),
                new FileDescriptor("test2.storage", 0, 0, 0, 0),
                expectedFoundDescriptor,
                new FileDescriptor("test4.storage", 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var itemExists = repository.Exists(expectedFoundDescriptor.FileName);

            // Assert
            Assert.True(itemExists);
        }

        [Fact]
        public void Exists_WhenItemNotExists_ShouldReturnFalse()
        {
            // Arrange
            var storageBuffer = new byte[10000];

            var descriptors = new[]
            {
                new FileDescriptor("test1.storage", 0, 0, 0, 0),
                new FileDescriptor("test2.storage", 0, 0, 0, 0),
                new FileDescriptor("test3.storage", 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var itemExists = repository.Exists("test4.storage");

            // Assert
            Assert.False(itemExists);
        }

        private static IFileDescriptorRepository CreateRepository(byte[] storageBuffer, bool initializeStorage, params FileDescriptor[] itemsToAdd)
        {
            var logger = new LoggerConfiguration().CreateLogger();

            var storageStreamProvider = StorageStreamProviderMockFactory.Create(storageBuffer);
            var storageConnection = new StorageConnection(storageStreamProvider, logger);

            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var filesystemDescriptorAccessor = new FilesystemDescriptorAccessor(storageConnection, filesystemDescriptorSerializer, logger);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var repository = new FileDescriptorRepository(storageConnection, filesystemDescriptorAccessor, fileDescriptorSerializer, logger);

            if (initializeStorage)
            {
                var storageInitializer =
                    new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);
                storageInitializer.Initialize(storageBuffer.Length, FileNameLength);
                var fileNameLength = filesystemDescriptorAccessor.Value.FileDescriptorLength - FileDescriptor.BytesWithoutFilename;

                var offset = -FilesystemDescriptor.BytesTotal - FileDescriptor.BytesWithoutFilename - fileNameLength;
                foreach (var itemToAdd in itemsToAdd)
                {
                    repository.Write(new StorageItem<FileDescriptor>(itemToAdd, new Cursor(offset, SeekOrigin.End)));
                    var currentFilesystemDescriptor = filesystemDescriptorAccessor.Value;
                    var updatedFilesystemDescriptor = currentFilesystemDescriptor
                        .WithFileDescriptorsCount(currentFilesystemDescriptor.FileDescriptorsCount + 1);
                    filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);
                    offset -= FileDescriptor.BytesWithoutFilename + fileNameLength;
                }
            }

            return repository;
        }
    }
}