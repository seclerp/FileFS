using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Allocation
{
    public class FileAllocatorTests
    {
        private const int FileNameLength = 100;

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(2000)]
        public void AllocateFile_WhenThereAreDataAndSizeIsNonZero_ShouldAllocateOnEnd(int dataSize)
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var storageSize = 10000;
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(storageSize, FileNameLength);
                var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
                var expectedCursor = new Cursor(dataSize * 2, SeekOrigin.Begin);

                // Act
                allocator.AllocateFile(dataSize, true);
                allocator.AllocateFile(dataSize, true);
                var cursor = allocator.AllocateFile(dataSize, true);

                // Assert
                Assert.Equal(expectedCursor, cursor);
            }
            finally
            {
                if (File.Exists(fileFsStorageName))
                {
                    File.Delete(fileFsStorageName);
                }
            }
        }

        [Fact]
        public void AllocateFile_WhenSizeIsZero_ShouldAlwaysReturnZeroCursor()
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var storageSize = 10000;
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(storageSize, FileNameLength);
                var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
                var expectedCursor = new Cursor(0, SeekOrigin.Begin);

                // Act
                var cursorA = allocator.AllocateFile(0, true);
                var cursorB = allocator.AllocateFile(0, true);
                var cursorC = allocator.AllocateFile(0, true);

                // Assert
                var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
                Assert.Equal(0, filesystemDescriptor.FilesDataLength);
                Assert.Equal(expectedCursor, cursorA);
                Assert.Equal(expectedCursor, cursorB);
                Assert.Equal(expectedCursor, cursorC);
            }
            finally
            {
                if (File.Exists(fileFsStorageName))
                {
                    File.Delete(fileFsStorageName);
                }
            }
        }

        [Fact]
        public void AllocateFile_WhenSuitableGapExists_ShouldAllocateInGap()
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var storageSize = 10000;
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(storageSize, FileNameLength);
                var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
                var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
                var dataBytes = Encoding.UTF8.GetBytes("data");
                fileRepository.Create(new FileEntry("/some_file", Guid.NewGuid(), dataBytes));
                fileRepository.Create(new FileEntry("/some_other_file", Guid.NewGuid(), dataBytes));
                fileRepository.Create(new FileEntry("/some_other_other_file", Guid.NewGuid(), dataBytes));
                fileRepository.Delete("/some_file");
                var expectedCursor = new Cursor(0, SeekOrigin.Begin);

                // Ac
                var cursor = allocator.AllocateFile(dataBytes.Length, true);

                // Assert
                var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
                Assert.Equal(dataBytes.Length * 3, filesystemDescriptor.FilesDataLength);
                Assert.Equal(expectedCursor, cursor);
            }
            finally
            {
                if (File.Exists(fileFsStorageName))
                {
                    File.Delete(fileFsStorageName);
                }
            }
        }

        [Fact]
        public void AllocateFile_WhenSuitableGapNotExists_ShouldAllocateOnEnd()
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var storageSize = 10000;
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(storageSize, FileNameLength);
                var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
                var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
                var dataBytes = Encoding.UTF8.GetBytes("data");
                fileRepository.Create(new FileEntry("some_file", Guid.NewGuid(), dataBytes));
                fileRepository.Create(new FileEntry("some_other_file", Guid.NewGuid(), dataBytes));
                fileRepository.Create(new FileEntry("some_other_other_file", Guid.NewGuid(), dataBytes));
                fileRepository.Delete("some_file");
                var expectedCursor = new Cursor(dataBytes.Length * 3, SeekOrigin.Begin);

                // Ac
                var cursor = allocator.AllocateFile(dataBytes.Length + 5, true);

                // Assert
                var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
                Assert.Equal((dataBytes.Length * 4) + 5, filesystemDescriptor.FilesDataLength);
                Assert.Equal(expectedCursor, cursor);
            }
            finally
            {
                if (File.Exists(fileFsStorageName))
                {
                    File.Delete(fileFsStorageName);
                }
            }
        }

        [Theory]
        [InlineData(10000, 160000, 100000)]
        public void AllocateFile_WhenDataSizeBiggerThanExistingSpace_ShouldTryOptimizeAndExtend(int storageSize, int expectedNewStorageSize, int dataSize)
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var storageOptimizerMock = new Mock<IStorageOptimizer>();
                var storageExtenderMock = new Mock<IStorageExtender>();
                var serviceProvider = CreateServiceProvider(fileFsStorageName, storageOptimizerMock.Object, storageExtenderMock.Object);
                serviceProvider.InitializeStorage(storageSize, FileNameLength);
                var allocator = serviceProvider.GetRequiredService<IFileAllocator>();

                // Ac
                allocator.AllocateFile(dataSize, true);

                // Assert
                storageOptimizerMock.Verify(s => s.Optimize());
                storageExtenderMock.Verify(s => s.Extend(expectedNewStorageSize));
            }
            finally
            {
                if (File.Exists(fileFsStorageName))
                {
                    File.Delete(fileFsStorageName);
                }
            }
        }

        private static IServiceProvider CreateServiceProvider(string storageFileName, IStorageOptimizer storageOptimizer = null, IStorageExtender storageExtender = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageFileName);

            storageOptimizer ??= new Mock<IStorageOptimizer>().Object;
            services.Replace(ServiceDescriptor.Singleton(storageOptimizer));

            storageExtender ??= new Mock<IStorageExtender>().Object;
            services.Replace(ServiceDescriptor.Singleton(storageExtender));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}