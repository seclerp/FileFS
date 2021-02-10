using System.IO;
using System.Text;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Memory;
using FileFS.DataAccess.Repositories;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.DataAccess.Serializers;
using FileFS.DataAccess.Tests.Repositories.Extensions;
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
            Assert.True(dataStream.StreamEquals(createdFileDataStream));
        }

        public void Update_FileEntry_ShouldUpdateFileEntry() {}

        public void Update_StreamedFileEntry_ShouldUpdateFileEntry() {}

        public void Read_FileEntry_ShouldReturnValidItem() {}

        public void Read_StreamedFileEntry_ShouldReturnValidItemAndWriteDataToStream() {}

        public void Rename_ShouldRenameSuccessfully() {}

        public void Delete_ShouldDeleteSuccessfully() {}

        public void Exists_WhenItemExists_ShouldReturnTrue() {}

        public void Exists_WhenItemNotExists_ShouldReturnFalse() {}

        public void GetAllFilesInfo_WhenThereAreNoData_ShouldReturnEmptyCollection() {}

        public void GetAllFilesInfo_WhenThereAreData_ShouldReturnValidCollection() {}

        private static IFileRepository CreateRepository(byte[] storageBuffer, bool initializeStorage)
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

            if (initializeStorage)
            {
                var storageInitializer =
                    new StorageInitializer(storageStreamProvider, filesystemDescriptorSerializer, logger);
                storageInitializer.Initialize(storageBuffer.Length, FileNameLength);
            }

            var fileRepository = new FileRepository(storageConnection, allocator, filesystemDescriptorAccessor, fileDescriptorRepository, logger);

            return fileRepository;
        }
    }
}