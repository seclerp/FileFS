﻿using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Abstractions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Tests.Extensions;
using FileFS.Tests.Shared.Comparers;
using Microsoft.Extensions.DependencyInjection;
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
        [InlineData("/file.name", "data")]
        public void Create_FileEntry_ShouldCreateValidFile(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(fileName, Guid.NewGuid(), dataBytes);
            var repository = CreateRepository(storageBuffer, true);

            // Act
            repository.Create(fileEntry);

            // Assert
            var createdFile = repository.Read(fileName);
            Assert.Equal(fileEntry.ParentEntryId, createdFile.ParentEntryId);
            Assert.Equal(fileEntry.EntryName, createdFile.EntryName);
            Assert.Equal(fileEntry.DataLength, createdFile.DataLength);
            Assert.Equal(fileEntry.Data, createdFile.Data);
        }

        [Theory]
        [InlineData("/file.name", 10_000_000)]
        public void Create_StreamedFileEntry_WithLongData_ShouldCreateValidFile(string fileName, int streamLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];
            var dataBuffer = new byte[streamLength];
            using var dataStream = new MemoryStream(dataBuffer);
            var streamedFileEntry = new StreamedFileEntry(fileName, Guid.NewGuid(), dataStream, (int)dataStream.Length);
            var repository = CreateRepository(storageBuffer, true);

            // Act
            repository.Create(streamedFileEntry);

            // Assert
            var createdFileBuffer = new byte[streamLength];
            using var createdFileDataStream = new MemoryStream(createdFileBuffer);
            repository.ReadData(fileName, createdFileDataStream);
            Assert.Equal(dataStream.Length, createdFileDataStream.Length);
            Assert.Equal(dataStream, createdFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("/file.name", "olddata", "newdata")]
        public void Update_FileEntry_ShouldUpdateFileEntry(string fileName, string oldData, string newData)
        {
            // Arrange
            var storageBuffer = new byte[10000];

            var id = Guid.NewGuid();
            var oldFataBytes = Encoding.UTF8.GetBytes(oldData);
            var oldFileEntry = new FileEntry(fileName, id, oldFataBytes);

            var newDataBytes = Encoding.UTF8.GetBytes(newData);
            var newFileEntry = new FileEntry(fileName, id, newDataBytes);

            var repository = CreateRepository(storageBuffer, true, oldFileEntry);

            // Act
            repository.Update(newFileEntry);

            // Assert
            var updatedFileEntry = repository.Read(fileName);
            Assert.Equal(newFileEntry.ParentEntryId, updatedFileEntry.ParentEntryId);
            Assert.Equal(newFileEntry.EntryName, updatedFileEntry.EntryName);
            Assert.Equal(newFileEntry.DataLength, updatedFileEntry.DataLength);
            Assert.Equal(newFileEntry.Data, updatedFileEntry.Data);
        }

        [Theory]
        [InlineData("/file.name", "olddata", 10_000_000)]
        public void Update_StreamedFileEntry_WithLongData_ShouldUpdateFileEntry(string fileName, string oldData, int newDataLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];

            var id = Guid.NewGuid();
            var oldDataBytes = Encoding.UTF8.GetBytes(oldData);
            var oldFileEntry = new FileEntry(fileName, id, oldDataBytes);

            var newDataBuffer = new byte[newDataLength];
            using var newDataStream = new MemoryStream(newDataBuffer);
            var newFileEntry = new StreamedFileEntry(fileName, id, newDataStream, (int)newDataStream.Length);

            var repository = CreateRepository(storageBuffer, true, oldFileEntry);

            // Act
            repository.Update(newFileEntry);

            // Assert
            var updatedFileBuffer = new byte[newDataLength];
            using var updatedFileDataStream = new MemoryStream(updatedFileBuffer);
            repository.ReadData(fileName, updatedFileDataStream);
            Assert.Equal(newFileEntry.DataLength, updatedFileDataStream.Length);
            Assert.Equal(newDataStream, updatedFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("/file.name", "data")]
        public void Read_FileEntry_ShouldReturnValidItem(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var expectedFileEntry = new FileEntry(fileName, Guid.NewGuid(), dataBytes);
            var repository = CreateRepository(storageBuffer, true, expectedFileEntry);

            // Act
            var writtenFileEntry = repository.Read(fileName);

            // Assert
            Assert.Equal(expectedFileEntry.ParentEntryId, writtenFileEntry.ParentEntryId);
            Assert.Equal(expectedFileEntry.EntryName, writtenFileEntry.EntryName);
            Assert.Equal(expectedFileEntry.DataLength, writtenFileEntry.DataLength);
            Assert.Equal(expectedFileEntry.Data, writtenFileEntry.Data);
        }

        [Theory]
        [InlineData("/file.name", 10_000_000)]
        public void Read_StreamedFileEntry_WithLargeData_ShouldReturnValidItemAndWriteDataToStream(string fileName, int streamLength)
        {
            // Arrange
            var storageBuffer = new byte[11_000_000];
            var dataBuffer = new byte[streamLength];
            using var dataStream = new MemoryStream(dataBuffer);
            var streamedFileEntry = new StreamedFileEntry(fileName, Guid.NewGuid(), dataStream, (int)dataStream.Length);
            var repository = CreateRepository(storageBuffer, true, streamedFileEntry);
            var createdFileBuffer = new byte[streamLength];
            using var createdFileDataStream = new MemoryStream(createdFileBuffer);

            // Act
            repository.ReadData(fileName, createdFileDataStream);

            // Assert
            Assert.Equal(dataStream.Length, createdFileDataStream.Length);
            Assert.Equal(dataStream, createdFileDataStream, new StreamComparer());
        }

        [Theory]
        [InlineData("/file.name", "data")]
        public void Exists_WhenItemExists_ShouldReturnTrue(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileEntry = new FileEntry(fileName, Guid.NewGuid(), dataBytes);
            var repository = CreateRepository(storageBuffer, true, fileEntry);

            // Act
            var exists = repository.Exists(fileName);

            // Assert
            Assert.True(exists);
        }

        [Theory]
        [InlineData("/file.name")]
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

        private static IFileRepository CreateRepository(byte[] storageBuffer, bool initializeStorage, params IFileEntry[] itemsToCreate)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);
            var serviceProvider = services.BuildServiceProvider();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();

            if (initializeStorage)
            {
                var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();
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