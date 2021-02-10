using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Abstractions;
using FileFS.DataAccess.Memory;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Tests.Comparers;
using FileFS.DataAccess.Tests.Factories;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Repositories
{
    public class FileRepositoryTests
    {
        private const int FileNameLength = 100;

        [Theory]
        [InlineData("file.name", "data")]
        public void Create_FileEntry_ShouldCreateValidFile(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(fileName, dataBytes);
            var repository = CreateRepository(storageBuffer, true);

            // Act
            repository.Create(fileEntry);

            // Assert
            var createdFile = repository.Read(fileName);
            Assert.Equal(fileEntry.FileName, createdFile.FileName);
            Assert.Equal(fileEntry.DataLength, createdFile.DataLength);
            Assert.Equal(fileEntry.Data, createdFile.Data);
        }

        [Theory]
        [InlineData("file.name", 10_000_000)]
        public void Create_StreamedFileEntry_WithLongData_ShouldCreateValidFile(string fileName, int streamLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];
            var dataBuffer = new byte[streamLength];
            using var dataStream = new MemoryStream(dataBuffer);
            var streamedFileEntry = new StreamedFileEntry(fileName, dataStream, (int)dataStream.Length);
            var repository = CreateRepository(storageBuffer, true);

            // Act
            repository.Create(streamedFileEntry);

            // Assert
            var createdFileBuffer = new byte[streamLength];
            using var createdFileDataStream = new MemoryStream(createdFileBuffer);
            repository.Read(fileName, createdFileDataStream);
            Assert.Equal(dataStream.Length, createdFileDataStream.Length);
            Assert.Equal(dataStream, createdFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("file.name", "olddata", "newdata")]
        public void Update_FileEntry_ShouldUpdateFileEntry(string fileName, string oldData, string newData)
        {
            // Arrange
            var storageBuffer = new byte[10000];

            var oldFataBytes = Encoding.UTF8.GetBytes(oldData);
            var oldFileEntry = new FileEntry(fileName, oldFataBytes);

            var newDataBytes = Encoding.UTF8.GetBytes(newData);
            var newFileEntry = new FileEntry(fileName, newDataBytes);

            var repository = CreateRepository(storageBuffer, true, oldFileEntry);

            // Act
            repository.Update(newFileEntry);

            // Assert
            var updatedFileEntry = repository.Read(fileName);
            Assert.Equal(newFileEntry.FileName, updatedFileEntry.FileName);
            Assert.Equal(newFileEntry.DataLength, updatedFileEntry.DataLength);
            Assert.Equal(newFileEntry.Data, updatedFileEntry.Data);
        }

        [Theory]
        [InlineData("file.name", "olddata", 10_000_000)]
        public void Update_StreamedFileEntry_WithLongData_ShouldUpdateFileEntry(string fileName, string oldData, int newDataLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];

            var oldDataBytes = Encoding.UTF8.GetBytes(oldData);
            var oldFileEntry = new FileEntry(fileName, oldDataBytes);

            var newDataBuffer = new byte[newDataLength];
            using var newDataStream = new MemoryStream(newDataBuffer);
            var newFileEntry = new StreamedFileEntry(fileName, newDataStream, (int)newDataStream.Length);

            var repository = CreateRepository(storageBuffer, true, oldFileEntry);

            // Act
            repository.Update(newFileEntry);

            // Assert
            var updatedFileBuffer = new byte[newDataLength];
            using var updatedFileDataStream = new MemoryStream(updatedFileBuffer);
            repository.Read(fileName, updatedFileDataStream);
            Assert.Equal(newFileEntry.DataLength, updatedFileDataStream.Length);
            Assert.Equal(newDataStream, updatedFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("file.name", "data")]
        public void Read_FileEntry_ShouldReturnValidItem(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var expectedFileEntry = new FileEntry(fileName, dataBytes);
            var repository = CreateRepository(storageBuffer, true, expectedFileEntry);

            // Act
            var writtenFileEntry = repository.Read(fileName);

            // Assert
            Assert.Equal(expectedFileEntry.FileName, writtenFileEntry.FileName);
            Assert.Equal(expectedFileEntry.DataLength, writtenFileEntry.DataLength);
            Assert.Equal(expectedFileEntry.Data, writtenFileEntry.Data);
        }

        [Theory]
        [InlineData("file.name", 10_000_000)]
        public void Read_StreamedFileEntry_WithLargeData_ShouldReturnValidItemAndWriteDataToStream(string fileName, int streamLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];
            var dataBuffer = new byte[streamLength];
            using var dataStream = new MemoryStream(dataBuffer);
            var streamedFileEntry = new StreamedFileEntry(fileName, dataStream, (int)dataStream.Length);
            var repository = CreateRepository(storageBuffer, true, streamedFileEntry);
            var createdFileBuffer = new byte[streamLength];
            using var createdFileDataStream = new MemoryStream(createdFileBuffer);

            // Act
            repository.Read(fileName, createdFileDataStream);

            // Assert
            Assert.Equal(dataStream.Length, createdFileDataStream.Length);
            Assert.Equal(dataStream, createdFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("old.file.name", "new.file.name", "data")]
        public void Rename_ShouldRenameSuccessfully(string oldFileName, string newFileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(oldFileName, dataBytes);
            var repository = CreateRepository(storageBuffer, true, fileEntry);

            // Act
            repository.Rename(oldFileName, newFileName);

            // Assert
            var renamedFile = repository.Read(newFileName);
            Assert.Equal(newFileName, renamedFile.FileName);
            Assert.Equal(fileEntry.DataLength, renamedFile.DataLength);
            Assert.Equal(fileEntry.Data, renamedFile.Data);
        }

        [Theory]
        [InlineData("file.name", "data")]
        public void Delete_ShouldDeleteSuccessfully(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(fileName, dataBytes);
            var repository = CreateRepository(storageBuffer, true, fileEntry);
            var existsBeforeDeletion = repository.Exists(fileName);

            // Act
            repository.Delete(fileName);

            // Assert
            var existsAfterDeletion = repository.Exists(fileName);
            Assert.True(existsBeforeDeletion);
            Assert.False(existsAfterDeletion);
        }

        [Theory]
        [InlineData("file.name", "data")]
        public void Exists_WhenItemExists_ShouldReturnTrue(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(fileName, dataBytes);
            var repository = CreateRepository(storageBuffer, true, fileEntry);

            // Act
            var exists = repository.Exists(fileName);

            // Assert
            Assert.True(exists);
        }

        [Theory]
        [InlineData("file.name")]
        public void Exists_WhenItemNotExists_ShouldReturnFalse(string fileName)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer, true);

            // Act
            var exists = repository.Exists(fileName);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void GetAllFilesInfo_WhenThereAreNoData_ShouldReturnEmptyCollection()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer, true);

            // Act
            var allFilesInfo = repository.GetAllFilesInfo();

            // Assert
            Assert.Empty(allFilesInfo);
        }

        [Fact]
        public void GetAllFilesInfo_WhenThereAreData_ShouldReturnValidCollection()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes("exampleData");
            var expectedFileEntries = new IFileEntry[]
            {
                new FileEntry("test1.file", dataBytes),
                new FileEntry("test2.file", dataBytes),
                new FileEntry("test3.file", dataBytes),
                new FileEntry("test4.file", dataBytes),
            };
            var expectedFileEntryInfos = new[]
            {
                new FileEntryInfo("test1.file", dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileEntryInfo("test2.file", dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileEntryInfo("test3.file", dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileEntryInfo("test4.file", dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
            };
            var repository = CreateRepository(storageBuffer, true, expectedFileEntries);

            // Act
            var allFilesInfo = repository.GetAllFilesInfo();

            // Assert
            Assert.Equal(expectedFileEntryInfos, allFilesInfo, new FileEntryInfoEqualityComparer());
        }

        private static IFileRepository CreateRepository(byte[] storageBuffer, bool initializeStorage, params IFileEntry[] itemsToCreate)
        {
            var logger = new LoggerConfiguration().CreateLogger();

            var storageStreamProvider = StorageStreamProviderMockFactory.Create(storageBuffer);
            var storageConnection = new StorageConnection(storageStreamProvider, logger);

            var filesystemDescriptorSerializer = new FilesystemDescriptorSerializer(logger);
            var filesystemDescriptorAccessor = new FilesystemDescriptorAccessor(storageConnection, filesystemDescriptorSerializer, logger);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemDescriptorAccessor, logger);
            var fileDescriptorRepository = new FileDescriptorRepository(storageConnection, filesystemDescriptorAccessor, fileDescriptorSerializer, logger);

            var optimizer = new StorageOptimizer(storageConnection, fileDescriptorRepository, logger);
            var allocator = new FileAllocator(storageConnection, filesystemDescriptorAccessor, fileDescriptorRepository, optimizer, logger);
            var fileRepository = new FileRepository(storageConnection, allocator, filesystemDescriptorAccessor, fileDescriptorRepository, logger);

            if (initializeStorage)
            {
                var storageInitializer =
                    new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);
                storageInitializer.Initialize(storageBuffer.Length, FileNameLength);

                foreach (var itemToCreate in itemsToCreate)
                {
                    switch (itemToCreate)
                    {
                        case FileEntry fileEntry:
                            fileRepository.Create(fileEntry);
                            break;
                        case StreamedFileEntry streamedFileEntry:
                            fileRepository.Create(streamedFileEntry);
                            break;
                    }
                }
            }

            return fileRepository;
        }
    }
}