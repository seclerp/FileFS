using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Tests.Factories;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.DataAccess.Tests
{
    public class StorageConnectionTests
    {
        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void PerformWriteBytes_ShouldContainWrittenBytes(string data, int offset, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = new byte[bufferSize];
            var storageConnection = CreateStorageConnection(storageBuffer);

            // Act
            storageConnection.PerformWrite(new Cursor(offset, SeekOrigin.Begin), dataBytes);

            // Assert
            var writtenDataBytes = new ReadOnlySpan<byte>(storageBuffer, offset, dataBytes.Length);
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void PerformWriteStream_ShouldContainWrittenBytes(string data, int offset, int bufferSize)
        {
            // Arrange
            var storageBuffer = new byte[bufferSize];
            var storageConnection = CreateStorageConnection(storageBuffer);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var sourceStream = PrepareStreamWithData(dataBytes, bufferSize);

            // Act
            storageConnection.PerformWrite(new Cursor(offset, SeekOrigin.Begin), dataBytes.Length, sourceStream);

            // Assert
            var writtenDataBytes = new ReadOnlySpan<byte>(storageBuffer, offset, dataBytes.Length);
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void PerformReadBytes_ShouldReturnWrittenBytes(string data, int offset, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = PrepareBufferWithData(dataBytes, offset, bufferSize);
            var storageConnection = CreateStorageConnection(storageBuffer);

            // Act
            var writtenDataBytes = storageConnection.PerformRead(new Cursor(offset, SeekOrigin.Begin), dataBytes.Length);

            // Assert
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void PerformReadStream_ShouldWriteBytesToDestination(string data, int offset, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = PrepareBufferWithData(dataBytes, offset, bufferSize);
            var storageConnection = CreateStorageConnection(storageBuffer);

            var destinationBuffer = new byte[bufferSize];
            using var destinationStream = new MemoryStream(destinationBuffer);

            // Act
            storageConnection.PerformRead(new Cursor(offset, SeekOrigin.Begin), dataBytes.Length, destinationStream);

            // Assert
            var writtenDataBytes = new ReadOnlySpan<byte>(destinationBuffer, 0, dataBytes.Length);
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 256, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 300, 1000)]
        [InlineData("", 5, 0, 6)]
        public void PerformCopy_ShouldContainCopiedData(string data, int offsetFrom, int offsetTo, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = PrepareBufferWithData(dataBytes, offsetFrom, bufferSize);
            var storageConnection = CreateStorageConnection(storageBuffer);

            // Act
            storageConnection.PerformCopy(new Cursor(offsetFrom, SeekOrigin.Begin), new Cursor(offsetTo, SeekOrigin.Begin), dataBytes.Length);

            // Assert
            var copiedDataBytes = new ReadOnlySpan<byte>(storageBuffer, offsetTo, dataBytes.Length);
            Assert.Equal(data, Encoding.UTF8.GetString(copiedDataBytes));
        }

        private static IStorageConnection CreateStorageConnection(byte[] storageBytes)
        {
            var logger = new LoggerConfiguration().CreateLogger();
            var storageStreamProvider = StorageStreamProviderMockFactory.Create(storageBytes);

            return new StorageConnection(storageStreamProvider, logger);
        }

        private static byte[] PrepareBufferWithData(byte[] data, int offset, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            using var stream = new MemoryStream(buffer);
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(data);

            return buffer;
        }

        private static Stream PrepareStreamWithData(byte[] dataBytes, int bufferSize)
        {
            var stream = new MemoryStream(bufferSize);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}