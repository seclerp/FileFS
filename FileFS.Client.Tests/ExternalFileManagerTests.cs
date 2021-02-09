using System;
using System.IO;
using System.Text;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Client.Tests
{
    public class ExternalFileManagerTests
    {
        private readonly string _testFileName;

        public ExternalFileManagerTests()
        {
            _testFileName = $"{Guid.NewGuid()}.txt";
        }

        [Theory]
        [InlineData("Hello, world!")]
        [InlineData("")]
        public void Write_ShouldWriteProvidedData(string text)
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var externalFileManager = new ExternalFileManager(logger);
                var textBytes = Encoding.UTF8.GetBytes(text);

                // Act
                externalFileManager.Write(_testFileName, textBytes);

                // Assert
                var writtenText = File.ReadAllText(_testFileName);
                Assert.Equal(text, writtenText);
            }
            finally
            {
                ClearSideEffects();
            }
        }

        [Theory]
        [InlineData("Hello, world!")]
        [InlineData("")]
        public void Read_ShouldReadAlreadyWrittenData(string text)
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var externalFileManager = new ExternalFileManager(logger);
                File.WriteAllText(_testFileName, text);

                // Act
                var writtenTextBytes = externalFileManager.Read(_testFileName);

                // Assert
                var writtenText = Encoding.UTF8.GetString(writtenTextBytes);
                Assert.Equal(text, writtenText);
            }
            finally
            {
                ClearSideEffects();
            }
        }

        [Theory]
        [InlineData("Hello, world!")]
        [InlineData("")]
        public void OpenWriteStream_ShouldOpenCorrectStream(string text)
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var externalFileManager = new ExternalFileManager(logger);

                // Act
                var writeStream = externalFileManager.OpenWriteStream(_testFileName);

                // Assert
                using var writer = new StreamWriter(writeStream);
                writer.Write(text);
                writer.Dispose();
                var writtenText = File.ReadAllText(_testFileName);

                Assert.Equal(text, writtenText);
            }
            finally
            {
                ClearSideEffects();
            }
        }

        [Theory]
        [InlineData("Hello, world!")]
        [InlineData("")]
        public void OpenReadStream_ShouldOpenCorrectStream(string text)
        {
            try
            {
                // Arrange
                var logger = new LoggerConfiguration().CreateLogger();
                var externalFileManager = new ExternalFileManager(logger);
                File.WriteAllText(_testFileName, text);

                // Act
                var readStream = externalFileManager.OpenReadStream(_testFileName);

                // Assert
                using var reader = new StreamReader(readStream);
                var writtenText = reader.ReadToEnd();

                Assert.Equal(text, writtenText);
            }
            finally
            {
                ClearSideEffects();
            }
        }

        private void ClearSideEffects()
        {
            if (File.Exists(_testFileName))
            {
                File.Delete(_testFileName);
            }
        }
    }
}