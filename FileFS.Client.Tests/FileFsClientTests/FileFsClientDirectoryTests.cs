using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;
using Moq;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Client.Tests.FileFsClientTests
{
    public class FileFsClientDirectoryTests
    {
        private readonly Mock<IEntryRepository> _entryRepositoryMock;
        private readonly Mock<IDirectoryRepository> _directoryRepositoryMock;
        private readonly Mock<ITransactionWrapper> _transactionWrapperMock;
        private readonly FileFsClient _client;

        public FileFsClientDirectoryTests()
        {
            _entryRepositoryMock = new Mock<IEntryRepository>();
            _directoryRepositoryMock = new Mock<IDirectoryRepository>();

            _directoryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            _transactionWrapperMock = new Mock<ITransactionWrapper>();
            _transactionWrapperMock.Setup(t => t.BeginTransaction());
            _transactionWrapperMock.Setup(t => t.EndTransaction());

            _client = new FileFsClient(
                null,
                _directoryRepositoryMock.Object,
                _entryRepositoryMock.Object,
                null,
                null,
                _transactionWrapperMock.Object);
        }

        [Theory]
        [InlineData("/some dir")]
        [InlineData("/some-dir")]
        [InlineData("/some-dir123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void CreateDirectory_WithValidParameters_ShouldCallCreate(string name)
        {
            // Act
            _client.CreateDirectory(name);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Create(It.IsAny<DirectoryEntry>()));
        }

        [Theory]
        [InlineData("/dir$name")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("()9")]
        [InlineData("+++")]
        [InlineData("#sdfd")]
        [InlineData("\\/\\/asdasd")]
        [InlineData("!!!asdasd!!!")]
        [InlineData("[dapk_xantep]")]
        [InlineData("&lol&")]
        [InlineData("%%")]
        public void CreateEmpty_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.CreateDirectory(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/dirname")]
        public void CreateEmpty_WhenEntryAlreadyExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);

            // Act
            void Act() => _client.CreateDirectory(fileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }
    }
}