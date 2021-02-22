using System;
using System.Text;
using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.Tests.Shared.Comparers;
using Moq;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Client.Tests.FileFsClientTests
{
    public class FileFsClientGenericTests
    {
        private readonly Mock<IEntryRepository> _entryRepositoryMock;
        private readonly Mock<IDirectoryRepository> _directoryRepositoryMock;
        private readonly Mock<ITransactionWrapper> _transactionWrapperMock;
        private readonly FileFsClient _client;

        public FileFsClientGenericTests()
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
        [InlineData("/from", "/to")]
        public void Rename_WithValidParameters_ShouldCallRename(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            // Act
            _client.Rename(oldFileName, newFileName);

            // Assert
            _entryRepositoryMock.Verify(r => r.Rename(oldFileName, newFileName));
        }

        [Theory]
        [InlineData("file$name", "/valid")]
        [InlineData("", "/valid")]
        [InlineData(null, "/valid")]
        [InlineData("()9", "/valid")]
        [InlineData("+++", "/valid")]
        [InlineData("#sdfd", "/valid")]
        [InlineData("\\/\\/asdasd", "/valid")]
        [InlineData("!!!asdasd!!!", "/valid")]
        [InlineData("[dapk_xantep]", "/valid")]
        [InlineData("&lol&", "/valid")]
        [InlineData("%%", "/valid")]
        [InlineData("/valid", "file$name")]
        [InlineData("/valid", "")]
        [InlineData("/valid", null)]
        [InlineData("/valid", "()9")]
        [InlineData("/valid", "+++")]
        [InlineData("/valid", "#sdfd")]
        [InlineData("/valid", "\\/\\/asdasd")]
        [InlineData("/valid", "!!!asdasd!!!")]
        [InlineData("/valid", "[dapk_xantep]")]
        [InlineData("/valid", "&lol&")]
        [InlineData("/valid", "%%")]
        [InlineData("432$$4", "%%")]
        public void Rename_WithOneOfNamesIsInvalid_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Act
            void Act() => _client.Rename(oldFileName, newFileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Rename_WhenEntryNotExists_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(false);

            // Act
            void Act() => _client.Rename(oldFileName, newFileName);

            // Assert
            Assert.Throws<EntryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Rename_WhenDestinationEntryExists_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(newFileName))
                .Returns(true);

            // Act
            void Act() => _client.Rename(oldFileName, newFileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/from/from", "/to/to")]
        public void Rename_WhenParentsNotMatch_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            // Act
            void Act() => _client.Rename(oldFileName, newFileName);

            // Assert
            Assert.Throws<ArgumentNonValidException>(Act);
        }

        [Theory]
        [InlineData("/to")]
        public void Rename_WhenEntryIsRootDirectory_ShouldThrowException(string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            // Act
            void Act() => _client.Rename(PathConstants.RootDirectoryName, newFileName);

            // Assert
            Assert.Throws<OperationIsInvalid>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Move_WithValidParameters_ShouldCallMove(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            // Act
            _client.Move(oldFileName, newFileName);

            // Assert
            _entryRepositoryMock.Verify(r => r.Move(oldFileName, newFileName));
        }

        [Theory]
        [InlineData("file$name", "/valid")]
        [InlineData("", "/valid")]
        [InlineData(null, "/valid")]
        [InlineData("()9", "/valid")]
        [InlineData("+++", "/valid")]
        [InlineData("#sdfd", "/valid")]
        [InlineData("\\/\\/asdasd", "/valid")]
        [InlineData("!!!asdasd!!!", "/valid")]
        [InlineData("[dapk_xantep]", "/valid")]
        [InlineData("&lol&", "/valid")]
        [InlineData("%%", "/valid")]
        [InlineData("/valid", "file$name")]
        [InlineData("/valid", "")]
        [InlineData("/valid", null)]
        [InlineData("/valid", "()9")]
        [InlineData("/valid", "+++")]
        [InlineData("/valid", "#sdfd")]
        [InlineData("/valid", "\\/\\/asdasd")]
        [InlineData("/valid", "!!!asdasd!!!")]
        [InlineData("/valid", "[dapk_xantep]")]
        [InlineData("/valid", "&lol&")]
        [InlineData("/valid", "%%")]
        [InlineData("432$$4", "%%")]
        public void Move_WithOneOfNamesIsInvalid_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Act
            void Act() => _client.Move(oldFileName, newFileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Move_WhenEntryNotExists_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(false);

            // Act
            void Act() => _client.Move(oldFileName, newFileName);

            // Assert
            Assert.Throws<EntryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Move_WhenDestinationEntryExists_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(newFileName))
                .Returns(true);

            // Act
            void Act() => _client.Move(oldFileName, newFileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to/to")]
        public void Move_WhenDestinationParentEntryNotExists_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(newFileName.GetParentFullName()))
                .Returns(false);

            _entryRepositoryMock
                .Setup(r => r.Exists(newFileName))
                .Returns(false);

            // Act
            void Act() => _client.Move(oldFileName, newFileName);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/to")]
        public void Move_WhenEntryIsRootDirectory_ShouldThrowException(string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            // Act
            void Act() => _client.Move(PathConstants.RootDirectoryName, newFileName);

            // Assert
            Assert.Throws<OperationIsInvalid>(Act);
        }

        [Theory]
        [InlineData("/from", "/from/to")]
        public void Move_WhenDestinationIsChildToSource_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldFileName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(newFileName.GetParentFullName()))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(newFileName))
                .Returns(false);

            // Act
            void Act() => _client.Move(oldFileName, newFileName);

            // Assert
            Assert.Throws<OperationIsInvalid>(Act);
        }

        [Theory]
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void Exists_WithValidParameters_ShouldCallExists(string fileName)
        {
            // Act
            _client.Exists(fileName);

            // Assert
            _entryRepositoryMock.Verify(r => r.Exists(fileName));
        }

        [Theory]
        [InlineData("/file$name")]
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
        public void Exists_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.Exists(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Fact]
        public void GetEntriesInfo_WhenThereAreFiles_ShouldCallGetAllFilesInfo()
        {
            // Arrange
            var firstFileName = "hello";
            var firstFileData = Encoding.UTF8.GetBytes("123423423423423423423");

            var secondFileName = "world";
            var secondFileData = Encoding.UTF8.GetBytes("123");

            var expectedListFiles = new[]
            {
                new FileFsEntryInfo(firstFileName, EntryType.File, firstFileData.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileFsEntryInfo(secondFileName, EntryType.File, secondFileData.Length, DateTime.UtcNow, DateTime.UtcNow),
            };

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(PathConstants.RootDirectoryName))
                .Returns(expectedListFiles);

            // Act
            var files = _client.GetEntries(PathConstants.RootDirectoryName);

            // Assert
            Assert.Equal(expectedListFiles, files, new FileEntryInfoEqualityComparer());
        }

        [Fact]
        public void GetEntriesInfo_WhenThereAreNoFiles_ShouldReturnEmptyCollection()
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(PathConstants.RootDirectoryName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            // Act
            var files = _client.GetEntries(PathConstants.RootDirectoryName);

            // Assert
            Assert.Empty(files);
        }
    }
}