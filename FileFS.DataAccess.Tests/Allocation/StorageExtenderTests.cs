using System;
using System.IO;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests.Allocation
{
    public class StorageExtenderTests
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
                allocator.AllocateFile(dataSize);
                allocator.AllocateFile(dataSize);
                var cursor = allocator.AllocateFile(dataSize);

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

        private static IServiceProvider CreateServiceProvider(string storageFileName)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            services.AddFileFsDataAccessInMemory(storageFileName);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}