using System;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
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
    public class DirectoryRepositoryTests
    {
        private const int FileNameLength = 100;

        [Theory]
        [InlineData("/dir.name")]
        public void CreateDirectory_ShouldCreateValidDirectory(string name)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var directoryEntry = new DirectoryEntry(Guid.NewGuid(), name, Guid.NewGuid());
            var repository = CreateRepository(storageBuffer);

            // Act
            repository.Create(directoryEntry);

            // Assert
            var createdFile = repository.Find(name);
            Assert.Equal(directoryEntry.ParentEntryId, createdFile.ParentEntryId);
            Assert.Equal(directoryEntry.EntryName, createdFile.EntryName);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void Find_WhenItemExists_ShouldReturnItem(string name)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var directoryEntry = new DirectoryEntry(Guid.NewGuid(), name, Guid.NewGuid());
            var repository = CreateRepository(storageBuffer, directoryEntry);

            // Act
            var writtenItem = repository.Find(name);

            // Assert
            Assert.Equal(directoryEntry, writtenItem);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void Find_WhenItemNotExists_ShouldThrowException(string name)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer);

            // Act
            void Act() => repository.Find(name);

            // Assert
            Assert.Throws<EntryDescriptorNotFound>(Act);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void Exists_WhenItemExists_ShouldReturnTrue(string name)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var directoryEntry = new DirectoryEntry(Guid.NewGuid(), name, Guid.NewGuid());
            var repository = CreateRepository(storageBuffer, directoryEntry);

            // Act
            var exists = repository.Exists(name);

            // Assert
            Assert.True(exists);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void Exists_WhenItemNotExists_ShouldReturnFalse(string name)
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var repository = CreateRepository(storageBuffer);

            // Act
            var exists = repository.Exists(name);

            // Assert
            Assert.False(exists);
        }

        private static IDirectoryRepository CreateRepository(byte[] storageBuffer, params DirectoryEntry[] itemsToCreate)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);
            var serviceProvider = services.BuildServiceProvider();
            var directoryRepository = serviceProvider.GetRequiredService<IDirectoryRepository>();

            var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();
            storageInitializer.Initialize(storageBuffer.Length, FileNameLength);

            foreach (var itemToCreate in itemsToCreate)
            {
                directoryRepository.Create(itemToCreate);
            }

            return directoryRepository;
        }
    }
}