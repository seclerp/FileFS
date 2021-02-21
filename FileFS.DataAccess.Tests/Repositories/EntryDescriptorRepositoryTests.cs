using System;
using System.IO;
using System.Linq;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Repositories
{
    public class EntryDescriptorRepositoryTests
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
            var entryDescriptor = new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), fileName, EntryType.File, createdOn, updatedOn, dataOffset, dataLength);
            var repository = CreateRepository(storageBuffer, true);
            var cursor = new Cursor(-FilesystemDescriptor.BytesTotal - EntryDescriptor.BytesWithoutFilename - FileNameLength, SeekOrigin.End);
            repository.Write(new StorageItem<EntryDescriptor>(entryDescriptor, cursor));

            // Act
            var writtenFileDescriptor = repository.Read(cursor);

            // Assert
            Assert.Equal(entryDescriptor, writtenFileDescriptor.Value);
            Assert.Equal(cursor, writtenFileDescriptor.Cursor);
        }

        [Fact]
        public void ReadAll_WhereThereAreNoDescriptors_ShouldReturnOnlyRootDirectory()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer, true);

            // Act
            var descriptors = repository.ReadAll();

            // Assert
            Assert.Single(descriptors, descriptor => descriptor.Value.Name == PathConstants.RootDirectoryName);
        }

        [Fact]
        public void ReadAll_WhenThereAreExistingItems_ShouldReturnCollectionWithAllItems()
        {
            // Arrange
            var storageBuffer = new byte[10000];

            var expectedDescriptors = new[]
            {
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test1.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test2.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test3.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test4.storage", EntryType.File, 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, expectedDescriptors);

            // Act
            var writtenCollection = repository.ReadAll();

            // Assert
            var actualWithoutRoot = writtenCollection
                .Select(storageItem => storageItem.Value)
                .Where(descriptor => descriptor.Name != PathConstants.RootDirectoryName);
            Assert.Equal(expectedDescriptors, actualWithoutRoot);
        }

        [Theory]
        [InlineData("test.filename", 0, 0, 0, 0)]
        [InlineData("", 0, 0, 0, 0)]
        [InlineData("123123123", 123, 321, 123, 312)]
        public void Write_StorageShouldContainCorrectItem(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var fileDescriptor = new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), fileName, EntryType.File, createdOn, updatedOn, dataOffset, dataLength);
            var repository = CreateRepository(storageBuffer, true);
            var cursor = new Cursor(-FilesystemDescriptor.BytesTotal - EntryDescriptor.BytesWithoutFilename - FileNameLength, SeekOrigin.End);

            // Act
            repository.Write(new StorageItem<EntryDescriptor>(fileDescriptor, cursor));

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
            var expectedFoundDescriptor = new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test3.storage", EntryType.File, 0, 0, 0, 0);

            var descriptors = new[]
            {
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test1.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test2.storage", EntryType.File, 0, 0, 0, 0),
                expectedFoundDescriptor,
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test4.storage", EntryType.File, 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var foundFileDescriptorItem = repository.Find(expectedFoundDescriptor.Name);

            // Assert
            Assert.Equal(expectedFoundDescriptor, foundFileDescriptorItem.Value);
        }

        [Theory]
        [InlineData("test.filename", 0, 0, 0, 0)]
        public void Exists_WhenItemExists_ShouldReturnTrue(string fileName, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var expectedFoundDescriptor = new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), fileName, EntryType.File, createdOn, updatedOn, dataOffset, dataLength);

            var descriptors = new[]
            {
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test1.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test2.storage", EntryType.File, 0, 0, 0, 0),
                expectedFoundDescriptor,
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "/test4.storage", EntryType.File, 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var itemExists = repository.Exists(expectedFoundDescriptor.Name);

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
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "test1.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "test2.storage", EntryType.File, 0, 0, 0, 0),
                new EntryDescriptor(Guid.NewGuid(), Guid.NewGuid(), "test3.storage", EntryType.File, 0, 0, 0, 0),
            };

            var repository = CreateRepository(storageBuffer, true, descriptors);

            // Act
            var itemExists = repository.Exists("/test4.storage");

            // Assert
            Assert.False(itemExists);
        }

        private static IEntryDescriptorRepository CreateRepository(byte[] storageBuffer, bool initializeStorage, params EntryDescriptor[] itemsToAdd)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);
            var serviceProvider = services.BuildServiceProvider();
            var entryDescriptorRepository = serviceProvider.GetRequiredService<IEntryDescriptorRepository>();

            if (initializeStorage)
            {
                var filesystemDescriptorAccessor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>();
                var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();

                storageInitializer.Initialize(storageBuffer.Length, FileNameLength);
                var fileNameLength = filesystemDescriptorAccessor.Value.EntryDescriptorLength - EntryDescriptor.BytesWithoutFilename;

                var offset = -FilesystemDescriptor.BytesTotal - ((EntryDescriptor.BytesWithoutFilename + fileNameLength) * 2);
                foreach (var itemToAdd in itemsToAdd)
                {
                    entryDescriptorRepository.Write(new StorageItem<EntryDescriptor>(itemToAdd, new Cursor(offset, SeekOrigin.End)));
                    var currentFilesystemDescriptor = filesystemDescriptorAccessor.Value;
                    var updatedFilesystemDescriptor = currentFilesystemDescriptor
                        .WithFileDescriptorsCount(currentFilesystemDescriptor.EntryDescriptorsCount + 1);
                    filesystemDescriptorAccessor.Update(updatedFilesystemDescriptor);
                    offset -= EntryDescriptor.BytesWithoutFilename + fileNameLength;
                }
            }

            return entryDescriptorRepository;
        }
    }
}