using System;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
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
    public class EntryRepositoryTests
    {
        private const int FileNameLength = 100;

        [Theory]
        [InlineData("/old.file.name", "/new.file.name", "data")]
        public void Rename_ShouldRenameSuccessfully(string oldFileName, string newFileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
            var directoryRepository = serviceProvider.GetRequiredService<IDirectoryRepository>();

            var dataBytes = Encoding.UTF8.GetBytes(data);
            InitializeStorage(serviceProvider, storageBuffer.Length);
            var rootDirectoryEntry = directoryRepository.Find(PathConstants.RootDirectoryName);
            var fileEntry = new FileEntry(oldFileName, rootDirectoryEntry.Id, dataBytes);
            fileRepository.Create(fileEntry);

            // Act
            entryRepository.Rename(oldFileName, newFileName);

            // Assert
            var renamedFile = fileRepository.Read(newFileName);
            Assert.Equal(newFileName, renamedFile.EntryName);
            Assert.Equal(fileEntry.ParentEntryId, renamedFile.ParentEntryId);
            Assert.Equal(fileEntry.DataLength, renamedFile.DataLength);
            Assert.Equal(fileEntry.Data, renamedFile.Data);
        }

        [Theory]
        [InlineData("/file.name", "data")]
        public void Delete_ShouldDeleteSuccessfully(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var parentEntryId = Guid.NewGuid();
            var fileEntry = new FileEntry(fileName, parentEntryId, dataBytes);
            InitializeStorage(serviceProvider, storageBuffer.Length, fileEntry);
            var existsBeforeDeletion = entryRepository.Exists(fileName);

            // Act
            entryRepository.Delete(fileName);

            // Assert
            var existsAfterDeletion = entryRepository.Exists(fileName);
            Assert.True(existsBeforeDeletion);
            Assert.False(existsAfterDeletion);
        }

        [Theory]
        [InlineData("/file.name", "data")]
        public void Exists_WhenItemExists_ShouldReturnTrue(string fileName, string data)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var parentEntryId = Guid.NewGuid();
            var fileEntry = new FileEntry(fileName, parentEntryId, dataBytes);
            InitializeStorage(serviceProvider, storageBuffer.Length, fileEntry);

            // Act
            var exists = entryRepository.Exists(fileName);

            // Assert
            Assert.True(exists);
        }

        [Theory]
        [InlineData("/file.name")]
        public void Exists_WhenItemNotExists_ShouldReturnFalse(string fileName)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();

            InitializeStorage(serviceProvider, storageBuffer.Length);

            // Act
            var exists = entryRepository.Exists(fileName);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void GetAllFilesInfo_WhenThereAreNoData_ShouldReturnEmptyCollection()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();

            InitializeStorage(serviceProvider, storageBuffer.Length);

            // Act
            var allFilesInfo = entryRepository.GetEntriesInfo(PathConstants.RootDirectoryName);

            // Assert
            Assert.Single(allFilesInfo, fileIntro => fileIntro.EntryName == PathConstants.RootDirectoryName);
        }

        [Fact]
        public void GetAllFilesInfo_WhenThereAreData_ShouldReturnValidCollection()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var entryRepository = serviceProvider.GetRequiredService<IEntryRepository>();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
            var directoryRepository = serviceProvider.GetRequiredService<IDirectoryRepository>();
            InitializeStorage(serviceProvider, storageBuffer.Length);

            var rootDirectory = directoryRepository.Find(PathConstants.RootDirectoryName);

            var dataBytes = Encoding.UTF8.GetBytes("exampleData");
            var expectedFileEntries = new[]
            {
                new FileEntry("/test1.file", rootDirectory.Id, dataBytes),
                new FileEntry("/test2.file", rootDirectory.Id, dataBytes),
                new FileEntry("/test3.file", rootDirectory.Id, dataBytes),
                new FileEntry("/test4.file", rootDirectory.Id, dataBytes),
            };
            var expectedFileEntryInfos = new[]
            {
                new FileFsEntryInfo("/", EntryType.Directory, 0, DateTime.UtcNow, DateTime.UtcNow),
                new FileFsEntryInfo("/test1.file", EntryType.File, dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileFsEntryInfo("/test2.file", EntryType.File, dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileFsEntryInfo("/test3.file", EntryType.File, dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileFsEntryInfo("/test4.file", EntryType.File, dataBytes.Length, DateTime.UtcNow, DateTime.UtcNow),
            };

            foreach (var fileEntry in expectedFileEntries)
            {
                fileRepository.Create(fileEntry);
            }

            // Act
            var allFilesInfo = entryRepository.GetEntriesInfo(PathConstants.RootDirectoryName);

            // Assert
            Assert.Equal(expectedFileEntryInfos, allFilesInfo, new FileEntryInfoEqualityComparer());
        }

        private static IServiceProvider CreateServiceProvider(byte[] storageBuffer)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static void InitializeStorage(IServiceProvider serviceProvider, int storageSize, params FileEntry[] itemsToCreate)
        {
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
            var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();

            storageInitializer.Initialize(storageSize, FileNameLength);

            foreach (var itemToCreate in itemsToCreate)
            {
                fileRepository.Create(itemToCreate);
            }
        }
    }
}