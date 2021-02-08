using System;
using System.IO;
using System.Text;
using FileFS.DataAccess.Abstractions;
using Moq;
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
        public void StorageConnection_PerformWriteBytes_ShouldContainWrittenBytes(string data, int offset, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = new byte[bufferSize];
            var storageConnection = CreateStorageConnection(storageBuffer);

            // Act
            storageConnection.PerformWrite(new Cursor(offset, SeekOrigin.Begin), dataBytes);
            var writtenBytes = new ReadOnlySpan<byte>(storageBuffer, offset, dataBytes.Length);

            // Assert
            Assert.Equal(data, Encoding.UTF8.GetString(writtenBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void StorageConnection_PerformWriteStream_ShouldContainWrittenBytes(string data, int offset, int bufferSize)
        {
            // Arrange
            var storageBuffer = new byte[bufferSize];
            var storageConnection = CreateStorageConnection(storageBuffer);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var sourceStream = PrepareStreamWithData(dataBytes, bufferSize);

            // Act
            storageConnection.PerformWrite(new Cursor(offset, SeekOrigin.Begin), dataBytes.Length, sourceStream);
            var writtenDataBytes = new ReadOnlySpan<byte>(storageBuffer, offset, dataBytes.Length);

            // Assert
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 1000)]
        [InlineData("", 5, 6)]
        public void StorageConnection_PerformReadBytes_ShouldReturnWrittenBytes(string data, int offset, int bufferSize)
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
        public void StorageConnection_PerformReadStream_ShouldWriteBytesToDestination(string data, int offset, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = PrepareBufferWithData(dataBytes, offset, bufferSize);
            var storageConnection = CreateStorageConnection(storageBuffer);

            var destinationBuffer = new byte[bufferSize];
            using var destinationStream = new MemoryStream(destinationBuffer);

            // Act
            storageConnection.PerformRead(new Cursor(offset, SeekOrigin.Begin), dataBytes.Length, destinationStream);
            var writtenDataBytes = new ReadOnlySpan<byte>(destinationBuffer, 0, dataBytes.Length);

            // Assert
            Assert.Equal(data, Encoding.UTF8.GetString(writtenDataBytes));
        }

        [Theory]
        [InlineData("Hello, World!", 128, 256, 1000)]
        [InlineData("Hello, World!2348203948 023 u402348750829347693rgsdhbkfbg", 0, 300, 1000)]
        [InlineData("", 5, 0, 6)]
        public void StorageConnection_PerformCopy_ShouldContainCopiedData(string data, int offsetFrom, int offsetTo, int bufferSize)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var storageBuffer = PrepareBufferWithData(dataBytes, offsetFrom, bufferSize);
            var storageConnection = CreateStorageConnection(storageBuffer);

            // Act
            storageConnection.PerformCopy(new Cursor(offsetFrom, SeekOrigin.Begin), new Cursor(offsetTo, SeekOrigin.Begin), dataBytes.Length);
            var copiedDataBytes = new ReadOnlySpan<byte>(storageBuffer, offsetTo, dataBytes.Length);

            // Assert
            Assert.Equal(data, Encoding.UTF8.GetString(copiedDataBytes));
        }

        private static IStorageConnection CreateStorageConnection(byte[] storageBytes)
        {
            var logger = new LoggerConfiguration().CreateLogger();

            var storageStreamProviderMock = new Mock<IStorageStreamProvider>();
            storageStreamProviderMock
                .Setup(provider => provider.OpenStream())
                .Returns(new MemoryStream(storageBytes));

            return new StorageConnection(storageStreamProviderMock.Object, logger);
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