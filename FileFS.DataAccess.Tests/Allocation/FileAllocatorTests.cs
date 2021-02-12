using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
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
            // Arrange
            var buffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(buffer);
            serviceProvider.InitializeStorage(buffer.Length, FileNameLength);
            var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
            var expectedCursor = new Cursor(dataSize * 2, SeekOrigin.Begin);

            // Act
            allocator.AllocateFile(dataSize);
            allocator.AllocateFile(dataSize);
            var cursor = allocator.AllocateFile(dataSize);

            // Assert
            Assert.Equal(expectedCursor, cursor);
        }

        [Fact]
        public void AllocateFile_WhenSizeIsZero_ShouldAlwaysReturnZeroCursor()
        {
            // Arrange
            var buffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(buffer);
            serviceProvider.InitializeStorage(buffer.Length, FileNameLength);
            var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
            var expectedCursor = new Cursor(0, SeekOrigin.Begin);

            // Act
            var cursorA = allocator.AllocateFile(0);
            var cursorB = allocator.AllocateFile(0);
            var cursorC = allocator.AllocateFile(0);

            // Assert
            var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
            Assert.Equal(0, filesystemDescriptor.FilesDataLength);
            Assert.Equal(expectedCursor, cursorA);
            Assert.Equal(expectedCursor, cursorB);
            Assert.Equal(expectedCursor, cursorC);
        }

        [Fact]
        public void AllocateFile_WhenSuitableGapExists_ShouldAllocateInGap()
        {
            // Arrange
            var buffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(buffer);
            serviceProvider.InitializeStorage(buffer.Length, FileNameLength);
            var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
            var dataBytes = Encoding.UTF8.GetBytes("data");
            fileRepository.Create(new FileEntry("some_file", dataBytes));
            fileRepository.Create(new FileEntry("some_other_file", dataBytes));
            fileRepository.Create(new FileEntry("some_other_other_file", dataBytes));
            fileRepository.Delete("some_file");
            var expectedCursor = new Cursor(0, SeekOrigin.Begin);

            // Ac
            var cursor = allocator.AllocateFile(dataBytes.Length);

            // Assert
            var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
            Assert.Equal(dataBytes.Length * 3, filesystemDescriptor.FilesDataLength);
            Assert.Equal(expectedCursor, cursor);
        }

        [Fact]
        public void AllocateFile_WhenSuitableGapNotExists_ShouldAllocateOnEnd()
        {
            // Arrange
            var buffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(buffer);
            serviceProvider.InitializeStorage(buffer.Length, FileNameLength);
            var allocator = serviceProvider.GetRequiredService<IFileAllocator>();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
            var dataBytes = Encoding.UTF8.GetBytes("data");
            fileRepository.Create(new FileEntry("some_file", dataBytes));
            fileRepository.Create(new FileEntry("some_other_file", dataBytes));
            fileRepository.Create(new FileEntry("some_other_other_file", dataBytes));
            fileRepository.Delete("some_file");
            var expectedCursor = new Cursor(dataBytes.Length * 3, SeekOrigin.Begin);

            // Ac
            var cursor = allocator.AllocateFile(dataBytes.Length + 5);

            // Assert
            var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
            Assert.Equal((dataBytes.Length * 4) + 5, filesystemDescriptor.FilesDataLength);
            Assert.Equal(expectedCursor, cursor);
        }

        [Theory]
        [InlineData(10000, 100000)]
        public void AllocateFile_WhenDataSizeBiggerThanExistingSpace_ShouldTryOptimizeAndThrowException(int storageSize, int dataSize)
        {
            // Arrange
            var buffer = new byte[storageSize];
            var storageOptimizerMock = new Mock<IStorageOptimizer>();
            var serviceProvider = CreateServiceProvider(buffer, storageOptimizerMock.Object);
            serviceProvider.InitializeStorage(buffer.Length, FileNameLength);
            var allocator = serviceProvider.GetRequiredService<IFileAllocator>();

            // Ac
            void Act() => allocator.AllocateFile(dataSize);

            // Assert
            Assert.Throws<NotEnoughSpaceException>(Act);
            storageOptimizerMock.Verify(s => s.Optimize());
        }

        private static IServiceProvider CreateServiceProvider(byte[] storageBuffer, IStorageOptimizer storageOptimizer = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageBuffer);

            storageOptimizer ??= new Mock<IStorageOptimizer>().Object;
            services.Replace(ServiceDescriptor.Singleton(storageOptimizer));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}