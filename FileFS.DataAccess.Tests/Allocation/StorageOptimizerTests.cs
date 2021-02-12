using System;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
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
    public class StorageOptimizerTests
    {
        private const int FileNameLength = 100;

        [Fact]
        public void Optimize_WhenGapsExists_ShouldCutThemOff()
        {
            // Arrange
            var storageBuffer = new byte[10000];
            var serviceProvider = CreateServiceProvider(storageBuffer);
            serviceProvider.InitializeStorage(storageBuffer.Length, FileNameLength);

            var optimizer = serviceProvider.GetRequiredService<IStorageOptimizer>();
            var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();

            var data = Encoding.UTF8.GetBytes("datatatata");
            var expectedLength = data.Length * 3;
            var expectedBytesOptimized = data.Length * 2;

            fileRepository.Create(new FileEntry("some_file", data));
            fileRepository.Create(new FileEntry("some_other_file", data));
            fileRepository.Create(new FileEntry("some_other_other_file", data));
            fileRepository.Create(new FileEntry("some_other_other_other_file1", data));
            fileRepository.Create(new FileEntry("some_other_other_other_file2", data));

            fileRepository.Delete("some_file");
            fileRepository.Delete("some_other_other_file");

            // Act
            var bytesOptimized = optimizer.Optimize();

            // Assert
            var filesystemDescriptor = serviceProvider.GetRequiredService<IFilesystemDescriptorAccessor>().Value;
            Assert.Equal(expectedLength, filesystemDescriptor.FilesDataLength);
            Assert.Equal(expectedBytesOptimized, bytesOptimized);
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