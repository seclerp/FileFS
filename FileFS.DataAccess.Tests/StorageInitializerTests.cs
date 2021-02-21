// Missing XML comment for publicly visible type or member...

using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Serializers.Abstractions;
using FileFS.DataAccess.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests
{
    public class StorageInitializerTests
    {
        [Theory]
        [InlineData(1000, 10)]
        [InlineData(100, 1)]
        [InlineData(100000, 1289)]
        public void Initialize_ShouldCreateStorageOfSizeAndFileNameLength(int size, int fileNameLength)
        {
            // Arrange
            var storageBuffer = new byte[size];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();
            var filesystemDescriptorSerializer = serviceProvider.GetRequiredService<ISerializer<FilesystemDescriptor>>();
            var entryDescriptorSerializer = serviceProvider.GetRequiredService<ISerializer<EntryDescriptor>>();
            var expectedFilesystemDescriptor =
                new FilesystemDescriptor(0, 1, fileNameLength + EntryDescriptor.BytesWithoutFilename);
            var expectedRootDirectoryName = PathConstants.RootDirectoryName;

            // Act
            storageInitializer.Initialize(size, fileNameLength);

            // Arrange
            using var newStorageStream = new MemoryStream(storageBuffer);
            newStorageStream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            var filesystemDescriptorBuffer = new byte[FilesystemDescriptor.BytesTotal];
            newStorageStream.Read(filesystemDescriptorBuffer);
            var filesystemDescriptor = filesystemDescriptorSerializer.FromBytes(filesystemDescriptorBuffer);
            newStorageStream.Seek(-FilesystemDescriptor.BytesTotal - fileNameLength - EntryDescriptor.BytesWithoutFilename, SeekOrigin.End);
            var rootEntryDescriptorBuffer = new byte[fileNameLength + EntryDescriptor.BytesWithoutFilename];
            newStorageStream.Read(rootEntryDescriptorBuffer);
            var rootEntryDescriptor = entryDescriptorSerializer.FromBytes(rootEntryDescriptorBuffer);
            Assert.Equal(size, newStorageStream.Length);
            Assert.Equal(expectedFilesystemDescriptor, filesystemDescriptor);
            Assert.Equal(rootEntryDescriptor.Id, rootEntryDescriptor.ParentId);
            Assert.Equal(expectedRootDirectoryName, rootEntryDescriptor.Name);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(100, 0)]
        [InlineData(0, 0)]
        [InlineData(0, -10)]
        [InlineData(-10, 0)]
        [InlineData(-10, -10)]
        public void Initialize_ShouldFailSizeOrFilenameLengthValidation(int size, int fileNameLength)
        {
            // Arrange
            var storageBuffer = new byte[0];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            var storageInitializer = serviceProvider.GetRequiredService<IStorageInitializer>();

            // Act
            void Act() => storageInitializer.Initialize(size, fileNameLength);

            // Arrange
            Assert.Throws<ArgumentNonValidException>(Act);
        }

        private static IServiceProvider CreateServiceProvider(byte[] storageBuffer)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}