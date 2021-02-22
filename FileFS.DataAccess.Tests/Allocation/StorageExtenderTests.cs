using System;
using System.IO;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Exceptions;
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
        private const int FileNameLength = 5;

        [Theory]
        [InlineData(100, 1000)]
        [InlineData(100, 101)]
        public void Extend_WhenNewSizeIsGreaterThanCurrent_ShouldExtend(int currentStorageSize, int newStorageSize)
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(currentStorageSize, FileNameLength);
                var filesystemDescriptorAccessor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>();
                var connection = serviceProvider.GetRequiredService<IStorageConnection>();
                var extender = serviceProvider.GetRequiredService<IStorageExtender>();
                var descriptor = filesystemDescriptorAccessor.Value;

                // Act
                extender.Extend(newStorageSize);

                // Assert
                var movedDescriptor = filesystemDescriptorAccessor.Value;
                Assert.Equal(newStorageSize, connection.GetSize());
                Assert.Equal(descriptor, movedDescriptor);
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
        [InlineData(100, 15)]
        [InlineData(100, 99)]
        public void Extend_WhenNewSizeIsEqualOrLessThanCurrent_ShouldThrowException(int currentStorageSize, int newStorageSize)
        {
            var fileFsStorageName = Guid.NewGuid().ToString();
            try
            {
                // Arrange
                var serviceProvider = CreateServiceProvider(fileFsStorageName);
                serviceProvider.InitializeStorage(currentStorageSize, FileNameLength);
                var extender = serviceProvider.GetRequiredService<IStorageExtender>();

                // Act
                void Act() => extender.Extend(newStorageSize);

                // Assert
                Assert.Throws<OperationIsInvalid>(Act);
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