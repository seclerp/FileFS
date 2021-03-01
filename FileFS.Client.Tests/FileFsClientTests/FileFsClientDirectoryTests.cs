using System;
using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Extensions;
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
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IDirectoryRepository> _directoryRepositoryMock;
        private readonly Mock<ITransactionWrapper> _transactionWrapperMock;
        private readonly FileFsClient _client;

        public FileFsClientDirectoryTests()
        {
            _entryRepositoryMock = new Mock<IEntryRepository>();
            _fileRepositoryMock = new Mock<IFileRepository>();
            _directoryRepositoryMock = new Mock<IDirectoryRepository>();

            _directoryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            _transactionWrapperMock = new Mock<ITransactionWrapper>();
            _transactionWrapperMock.Setup(t => t.BeginTransaction());
            _transactionWrapperMock.Setup(t => t.EndTransaction());

            _client = new FileFsClient(
                _fileRepositoryMock.Object,
                _directoryRepositoryMock.Object,
                _entryRepositoryMock.Object,
                null,
                null,
                _transactionWrapperMock.Object,
                new StorageOperationLocker());
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
        public void CreateDirectory_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.CreateDirectory(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/dirname")]
        public void CreateDirectory_WhenEntryAlreadyExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.CreateDirectory(fileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/dir", "/dir/name")]
        public void CreateDirectory_WhenParentEntryNotExists_ShouldThrowException(string parentName, string name)
        {
            // Arrange
            _directoryRepositoryMock
                .Setup(r => r.Exists(parentName))
                .Returns(false);

            // Act
            void Act() => _client.CreateDirectory(name);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Fact]
        public void Delete_WhenNameIsRootDirectory_ShouldThrowException()
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            // Act
            void Act() => _client.Delete(PathConstants.RootDirectoryName);

            // Assert
            Assert.Throws<ArgumentNonValidException>(Act);
        }

        [Theory]
        [InlineData("/dirname")]
        public void Delete_WhenDirectoryIsEmpty_ShouldCallDelete(string name)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(name))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            // Act
            _client.Delete(name);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Delete(name));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested")]
        public void Delete_WhenDirectoryHasDirectory_ShouldCallDelete(string name, string nextedDirectoryName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nextedDirectoryName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(name))
                .Returns(new[] { new FileFsEntryInfo(nextedDirectoryName, EntryType.Directory, 0, DateTime.UtcNow, DateTime.UtcNow) });

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(nextedDirectoryName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(nextedDirectoryName))
                .Returns(true);

            // Act
            _client.Delete(name);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Delete(name));
            _directoryRepositoryMock.Verify(r => r.Delete(nextedDirectoryName));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested")]
        public void Delete_WhenDirectoryHasFile_ShouldCallDelete(string name, string nestedFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(name))
                .Returns(new[] { new FileFsEntryInfo(nestedFileName, EntryType.File, 0, DateTime.UtcNow, DateTime.UtcNow) });

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _fileRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            // Act
            _client.Delete(name);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Delete(name));
            _entryRepositoryMock.Verify(r => r.Delete(nestedFileName));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested-dir", "/dirname/nested-file")]
        public void Delete_WhenDirectoryHasDirectoryAndFile_ShouldCallDelete(string name, string nestedDirectoryName, string nestedFileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedDirectoryName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(name))
                .Returns(new[]
                {
                    new FileFsEntryInfo(nestedDirectoryName, EntryType.Directory, 0, DateTime.UtcNow, DateTime.UtcNow),
                    new FileFsEntryInfo(nestedFileName, EntryType.File, 0, DateTime.UtcNow, DateTime.UtcNow),
                });

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(nestedDirectoryName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(nestedDirectoryName))
                .Returns(true);

            _fileRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            // Act
            _client.Delete(name);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Delete(name));
            _directoryRepositoryMock.Verify(r => r.Delete(nestedDirectoryName));
            _entryRepositoryMock.Verify(r => r.Delete(nestedFileName));
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Copy_WithValidParameters_ShouldCallMove(string fromName, string toName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _fileRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            // Act
            _client.Copy(fromName, toName);

            // Assert
            _fileRepositoryMock.Verify(r => r.Copy(fromName, toName));
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Copy_WhenDirectoryIsEmpty_ShouldCallDelete(string fromName, string toName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(fromName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            // Act
            _client.Copy(fromName, toName);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName == toName)));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested", "/destination")]
        public void Copy_WhenDirectoryHasDirectory_ShouldCallDelete(string fromName, string fromNextedDirectoryName, string toName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(fromNextedDirectoryName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(fromName))
                .Returns(new[] { new FileFsEntryInfo(fromNextedDirectoryName, EntryType.Directory, 0, DateTime.UtcNow, DateTime.UtcNow) });

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(fromNextedDirectoryName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(fromNextedDirectoryName))
                .Returns(true);

            // Act
            _client.Copy(fromName, toName);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName == toName)));
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName.GetShortName() == fromNextedDirectoryName.GetShortName())));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested", "/destination")]
        public void Copy_WhenDirectoryHasFile_ShouldCallDelete(string fromName, string nestedFileName, string toName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(fromName))
                .Returns(new[] { new FileFsEntryInfo(nestedFileName, EntryType.File, 0, DateTime.UtcNow, DateTime.UtcNow) });

            _directoryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _fileRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            // Act
            _client.Copy(fromName, toName);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName == toName)));
            _fileRepositoryMock.Verify(r => r.Copy(nestedFileName, It.Is<string>(st => st.GetShortName() == nestedFileName.GetShortName())));
        }

        [Theory]
        [InlineData("/dirname", "/dirname/nested-dir", "/dirname/nested-file", "/destination")]
        public void Copy_WhenDirectoryHasDirectoryAndFile_ShouldCallDelete(string fromName, string nestedDirectoryName, string nestedFileName, string toName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedDirectoryName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(fromName))
                .Returns(new[]
                {
                    new FileFsEntryInfo(nestedDirectoryName, EntryType.Directory, 0, DateTime.UtcNow, DateTime.UtcNow),
                    new FileFsEntryInfo(nestedFileName, EntryType.File, 0, DateTime.UtcNow, DateTime.UtcNow),
                });

            _entryRepositoryMock
                .Setup(r => r.GetEntriesInfo(nestedDirectoryName))
                .Returns(Array.Empty<FileFsEntryInfo>());

            _directoryRepositoryMock
                .Setup(r => r.Exists(fromName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(nestedDirectoryName))
                .Returns(true);

            _fileRepositoryMock
                .Setup(r => r.Exists(nestedFileName))
                .Returns(true);

            // Act
            _client.Copy(fromName, toName);

            // Assert
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName == toName)));
            _directoryRepositoryMock.Verify(r => r.Create(It.Is<DirectoryEntry>(entry => entry.EntryName.GetShortName() == nestedDirectoryName.GetShortName())));
            _fileRepositoryMock.Verify(r => r.Copy(nestedFileName, It.Is<string>(st => st.GetShortName() == nestedFileName.GetShortName())));
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
        public void DirectoryExists_WithInvalidFileName_ShouldThrowException(string name)
        {
            // Act
            void Act() => _client.DirectoryExists(name);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void DirectoryExists_WhenDirectoryExists_ShouldReturnTrue(string name)
        {
            // Arrange
            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            // Act
            var directoryExists = _client.DirectoryExists(name);

            // Assert
            Assert.True(directoryExists);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void DirectoryExists_WhenDirectoryNotExists_ShouldReturnFalse(string name)
        {
            // Arrange
            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(false);

            // Act
            var directoryExists = _client.DirectoryExists(name);

            // Assert
            Assert.False(directoryExists);
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
        public void IsDirectory_WithInvalidFileName_ShouldThrowException(string name)
        {
            // Act
            void Act() => _client.IsDirectory(name);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void IsDirectory_WhenEntryNotExists_ShouldThrowException(string name)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(false);

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(false);

            // Act
            void Act() => _client.IsDirectory(name);

            // Assert
            Assert.Throws<EntryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void IsDirectory_WhenDirectoryExists_ShouldReturnTrue(string name)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            // Act
            var directoryExists = _client.IsDirectory(name);

            // Assert
            Assert.True(directoryExists);
        }

        [Theory]
        [InlineData("/dir.name")]
        public void IsDirectory_WhenDirectoryNotExists_ShouldReturnFalse(string name)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(name))
                .Returns(false);

            // Act
            var directoryExists = _client.IsDirectory(name);

            // Assert
            Assert.False(directoryExists);
        }
    }
}