using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Exceptions;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests
{
    public class StorageStreamProviderTests
    {
        private readonly string _testStorageFileName;

        public StorageStreamProviderTests()
        {
            _testStorageFileName = $"{Guid.NewGuid()}.txt";
        }

        [Theory]
        [InlineData("Hello, world!")]
        public void OpenStream_WhenFileExistsAndCheckExistenceTrue_ShouldReturnCorrectStream(string text)
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var storageStreamProvider = new StorageStreamProvider(_testStorageFileName, logger);
                File.WriteAllText(_testStorageFileName, string.Empty);

                // Act
                using var storageStream = storageStreamProvider.OpenStream();

                // Assert
                using var writer = new BinaryWriter(storageStream);
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(text);
                writer.Dispose();
                using var reader = new BinaryReader(File.OpenRead(_testStorageFileName));
                var writtenTextBytes = reader.ReadBytes((int)reader.BaseStream.Length);
                var writtenText = Encoding.UTF8.GetString(writtenTextBytes);

                Assert.Equal(text, writtenText.Trim());
            }
            finally
            {
                ClearSideEffects();
            }
        }

        [Fact]
        public void OpenStream_WhenFileNotExistsAndCheckExistenceTrue_ShouldThrowException()
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var storageStreamProvider = new StorageStreamProvider(_testStorageFileName, logger);

                // Act
                void Act() => storageStreamProvider.OpenStream().Dispose();

                // Assert
                Assert.Throws<StorageNotFoundException>(Act);
            }
            finally
            {
                ClearSideEffects();
            }
        }

        private void ClearSideEffects()
        {
            if (File.Exists(_testStorageFileName))
            {
                File.Delete(_testStorageFileName);
            }
        }
    }
}