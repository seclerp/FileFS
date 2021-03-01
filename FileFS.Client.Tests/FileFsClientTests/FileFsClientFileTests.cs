using System.IO;
using System.Text;
using FileFS.Client.Abstractions;
using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using Moq;
using Xunit;
using DirectoryNotFoundException = FileFS.Client.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = FileFS.Client.Exceptions.FileNotFoundException;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Client.Tests.FileFsClientTests
{
    public class FileFsClientFileTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IExternalFileManager> _externalFileManagerMock;
        private readonly Mock<IStorageOptimizer> _storageOptimizerMock;
        private readonly Mock<IEntryRepository> _entryRepositoryMock;
        private readonly Mock<IDirectoryRepository> _directoryRepositoryMock;
        private readonly Mock<ITransactionWrapper> _transactionWrapperMock;
        private readonly FileFsClient _client;

        public FileFsClientFileTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _externalFileManagerMock = new Mock<IExternalFileManager>();
            _storageOptimizerMock = new Mock<IStorageOptimizer>();
            _entryRepositoryMock = new Mock<IEntryRepository>();
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
                _externalFileManagerMock.Object,
                _storageOptimizerMock.Object,
                _transactionWrapperMock.Object,
                new StorageOperationLocker());
        }

        [Theory]
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void CreateEmpty_WithValidParameters_ShouldCallCreate(string fileName)
        {
            // Act
            _client.CreateFile(fileName);

            // Assert
            _fileRepositoryMock.Verify(r => r.Create(It.IsAny<FileEntry>()));
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
        public void CreateEmpty_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.CreateFile(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void CreateEmpty_WhenEntryAlreadyExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.CreateFile(fileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/dir", "/dir/name")]
        public void CreateEmpty_WhenParentEntryNotExists_ShouldThrowException(string parentName, string name)
        {
            // Arrange
            _directoryRepositoryMock
                .Setup(r => r.Exists(parentName))
                .Returns(false);

            // Act
            void Act() => _client.CreateFile(name);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename", "")]
        [InlineData("/some-filename", "123123123")]
        [InlineData("/some-filename123", "123123123")]
        [InlineData("/1.2.3", "asdasd")]
        [InlineData("/123_123", "asdasd")]
        [InlineData("/_", "asdasd")]
        [InlineData("/a", "12312312")]
        public void Create_WithValidParameters_ShouldCallCreate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);

            // Act
            _client.CreateFile(fileName, dataBytes);

            // Assert
            _fileRepositoryMock.Verify(r => r.Create(It.IsAny<FileEntry>()));
        }

        [Theory]
        [InlineData("/file$name", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("()9", "")]
        [InlineData("+++", "")]
        [InlineData("#sdfd", "")]
        [InlineData("\\/\\/asdasd", "")]
        [InlineData("!!!asdasd!!!", "")]
        [InlineData("[dapk_xantep]", "")]
        [InlineData("&lol&", "")]
        [InlineData("%%", "")]
        public void Create_WithInvalidFileName_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);

            // Act
            void Act() => _client.CreateFile(fileName, dataBytes);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "")]
        public void Create_WhenEntryAlreadyExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.CreateFile(fileName, dataBytes);

            // Assert
            _fileRepositoryMock.VerifyAll();
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void Create_WithNullData_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.CreateFile(fileName, null);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("/dir", "/dir/name")]
        public void Create_WhenParentEntryNotExists_ShouldThrowException(string parentName, string name)
        {
            // Arrange
            _directoryRepositoryMock
                .Setup(r => r.Exists(parentName))
                .Returns(false);

            // Act
            void Act() => _client.CreateFile(name, Encoding.UTF8.GetBytes("data"));

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename", "")]
        [InlineData("/some-filename", "123123123")]
        [InlineData("/some-filename123", "123123123")]
        [InlineData("/1.2.3", "asdasd")]
        [InlineData("/123_123", "asdasd")]
        [InlineData("/_", "asdasd")]
        [InlineData("/a", "12312312")]
        public void CreateStreamed_WithValidParameters_ShouldCallCreate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            // Act
            _client.CreateFile(fileName, stream, dataBytes.Length);

            // Assert
            _fileRepositoryMock.Verify(r => r.Create(It.IsAny<StreamedFileEntry>()));
        }

        [Theory]
        [InlineData("/file$name", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("()9", "")]
        [InlineData("+++", "")]
        [InlineData("#sdfd", "")]
        [InlineData("\\/\\/asdasd", "")]
        [InlineData("!!!asdasd!!!", "")]
        [InlineData("[dapk_xantep]", "")]
        [InlineData("&lol&", "")]
        [InlineData("%%", "")]
        public void CreateStreamed_WithInvalidFileName_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            // Act
            void Act() => _client.CreateFile(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "")]
        public void CreateStreamed_WhenFileAlreadyExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.CreateFile(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void CreateStreamed_WithNullDataStream_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.CreateFile(fileName, null, 0);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("/dir", "/dir/name")]
        public void CreateStreamed_WhenParentEntryNotExists_ShouldThrowException(string parentName, string name)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes("data");
            using var stream = new MemoryStream(dataBytes);

            _directoryRepositoryMock
                .Setup(r => r.Exists(parentName))
                .Returns(false);

            // Act
            void Act() => _client.CreateFile(name, stream, dataBytes.Length);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename", "")]
        [InlineData("/some-filename", "123123123")]
        [InlineData("/some-filename123", "123123123")]
        [InlineData("/1.2.3", "asdasd")]
        [InlineData("/123_123", "asdasd")]
        [InlineData("/_", "asdasd")]
        [InlineData("/a", "12312312")]
        public void Update_WithValidParameters_ShouldCallUpdate(string fileName, string newData)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(newData);

            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            _client.Update(fileName, dataBytes);

            // Assert
            _fileRepositoryMock.Verify(r => r.Update(It.IsAny<FileEntry>()));
        }

        [Theory]
        [InlineData("/file$name", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("()9", "")]
        [InlineData("+++", "")]
        [InlineData("#sdfd", "")]
        [InlineData("\\/\\/asdasd", "")]
        [InlineData("!!!asdasd!!!", "")]
        [InlineData("[dapk_xantep]", "")]
        [InlineData("&lol&", "")]
        [InlineData("%%", "")]
        public void Update_WithInvalidFileName_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);

            // Act
            void Act() => _client.Update(fileName, dataBytes);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "data")]
        public void Update_WhenFileNotExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);

            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.Update(fileName, dataBytes);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void Update_WithNullData_ShouldThrowException(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.Update(fileName, null);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("/some filename", "")]
        [InlineData("/some-filename", "123123123")]
        [InlineData("/some-filename123", "123123123")]
        [InlineData("/1.2.3", "asdasd")]
        [InlineData("/123_123", "asdasd")]
        [InlineData("/_", "asdasd")]
        [InlineData("/a", "12312312")]
        public void UpdateStreamed_WithValidParameters_ShouldCallUpdate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            _client.Update(fileName, stream, dataBytes.Length);

            // Assert
            _fileRepositoryMock.Verify(r => r.Update(It.IsAny<StreamedFileEntry>()));
        }

        [Theory]
        [InlineData("/file$name", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("()9", "")]
        [InlineData("+++", "")]
        [InlineData("#sdfd", "")]
        [InlineData("\\/\\/asdasd", "")]
        [InlineData("!!!asdasd!!!", "")]
        [InlineData("[dapk_xantep]", "")]
        [InlineData("&lol&", "")]
        [InlineData("%%", "")]
        public void UpdateStreamed_WithInvalidFileName_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            // Act
            void Act() => _client.Update(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "data")]
        public void UpdateStreamed_WhenFileNotExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.Update(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void UpdateStreamed_WithNullDataStream_ShouldThrowException(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.Update(fileName, null, 0);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
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
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void Read_WithValidParameters_ShouldCallRead(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            _client.Read(fileName);

            // Assert
            _fileRepositoryMock.Verify(r => r.Read(fileName));
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
        public void Read_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.Read(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/file-name")]
        public void Read_WhenFileNotExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.Read(fileName);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void ReadStreamed_WithValidParameters_ShouldCallRead(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            _client.Read(fileName, new MemoryStream());

            // Assert
            _fileRepositoryMock.Verify(r => r.ReadData(fileName, It.IsAny<Stream>()));
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
        public void ReadStreamed_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.Read(fileName, new MemoryStream());

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void ReadStreamed_WithDestinationStreamNull_ShouldThrowException(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.Read(fileName, null);

            // Arrange
            Assert.Throws<ArgumentNonValidException>(Act);
        }

        [Theory]
        [InlineData("/file-name")]
        public void ReadStreamed_WhenFileNotExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.Read(fileName, new MemoryStream());

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void Delete_WithValidParameters_ShouldDeleteFile(string fileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            _client.Delete(fileName);

            // Assert
            _entryRepositoryMock.Verify(r => r.Delete(fileName));
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
        public void Delete_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.Delete(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename")]
        public void Delete_WhenFileNotExists_ShouldThrowException(string fileName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.Delete(fileName);

            // Assert
            Assert.Throws<EntryNotFoundException>(Act);
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
        public void Copy_WithOneOfNamesIsInvalid_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Act
            void Act() => _client.Copy(oldFileName, newFileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to")]
        public void Copy_WhenEntryNotExists_ShouldThrowException(string oldName, string newName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldName))
                .Returns(false);

            // Act
            void Act() => _client.Copy(oldName, newName);

            // Assert
            Assert.Throws<EntryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/from", "/to/to")]
        public void Copy_WhenDestinationParentEntryNotExists_ShouldThrowException(string oldName, string newName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(newName.GetParentFullName()))
                .Returns(false);

            _entryRepositoryMock
                .Setup(r => r.Exists(newName))
                .Returns(false);

            // Act
            void Act() => _client.Copy(oldName, newName);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/to")]
        public void Copy_WhenEntryIsRootDirectory_ShouldThrowException(string newName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(PathConstants.RootDirectoryName))
                .Returns(true);

            // Act
            void Act() => _client.Copy(PathConstants.RootDirectoryName, newName);

            // Assert
            Assert.Throws<OperationIsInvalid>(Act);
        }

        [Theory]
        [InlineData("/from", "/from/to")]
        public void Copy_WhenDestinationIsChildToSource_ShouldThrowException(string oldName, string newName)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(oldName))
                .Returns(true);

            _directoryRepositoryMock
                .Setup(r => r.Exists(newName.GetParentFullName()))
                .Returns(true);

            _entryRepositoryMock
                .Setup(r => r.Exists(newName))
                .Returns(false);

            // Act
            void Act() => _client.Copy(oldName, newName);

            // Assert
            Assert.Throws<OperationIsInvalid>(Act);
        }

        [Theory]
        [InlineData("/some filename", "external")]
        [InlineData("/some-filename", "external")]
        [InlineData("/some-filename123", "external")]
        [InlineData("/1.2.3", "external")]
        [InlineData("/123_123", "external")]
        [InlineData("/_", "external")]
        [InlineData("/a", "external")]
        public void Import_WithValidParameters_ShouldImportFile(string fileName, string externalPath)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            _externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(true);
            _externalFileManagerMock
                .Setup(e => e.OpenReadStream(externalPath))
                .Returns(new MemoryStream());

            // Act
            _client.ImportFile(externalPath, fileName);

            // Assert
            _fileRepositoryMock.Verify(r => r.Create(It.Is<StreamedFileEntry>(fileEntry => fileEntry.EntryName == fileName)));
        }

        [Theory]
        [InlineData("/file$name", "external")]
        [InlineData("", "external")]
        [InlineData(null, "external")]
        [InlineData("()9", "external")]
        [InlineData("+++", "external")]
        [InlineData("#sdfd", "external")]
        [InlineData("\\/\\/asdasd", "external")]
        [InlineData("!!!asdasd!!!", "external")]
        [InlineData("[dapk_xantep]", "external")]
        [InlineData("&lol&", "external")]
        [InlineData("%%", "external")]
        public void Import_WithInvalidFileName_ShouldThrowException(string fileName, string externalPath)
        {
            // Act
            void Act() => _client.ImportFile(externalPath, fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "external")]
        public void Import_WhenFileAlreadyExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            _entryRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            // Act
            void Act() => _client.ImportFile(externalPath, fileName);

            // Assert
            Assert.Throws<EntryAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/filename", "external")]
        public void Import_WhenExternalFileNotExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(false);

            // Act
            void Act() => _client.ImportFile(externalPath, fileName);

            // Assert
            Assert.Throws<ExternalFileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/some filename", "external")]
        [InlineData("/some-filename", "external")]
        [InlineData("/some-filename123", "external")]
        [InlineData("/1.2.3", "external")]
        [InlineData("/123_123", "external")]
        [InlineData("/_", "external")]
        [InlineData("/a", "external")]
        public void Export_WithValidParameters_ShouldExportFile(string fileName, string externalPath)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(false);
            externalFileManagerMock
                .Setup(e => e.OpenWriteStream(externalPath))
                .Returns(new MemoryStream());

            // Act
            _client.ExportFile(fileName, externalPath);

            // Assert
            _fileRepositoryMock.Verify(r => r.ReadData(fileName, It.IsAny<Stream>()));
        }

        [Theory]
        [InlineData("/file$name", "external")]
        [InlineData("", "external")]
        [InlineData(null, "external")]
        [InlineData("()9", "external")]
        [InlineData("+++", "external")]
        [InlineData("#sdfd", "external")]
        [InlineData("\\/\\/asdasd", "external")]
        [InlineData("!!!asdasd!!!", "external")]
        [InlineData("[dapk_xantep]", "external")]
        [InlineData("&lol&", "external")]
        [InlineData("%%", "external")]
        public void Export_WithInvalidFileName_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange

            // Act
            void Act() => _client.ExportFile(fileName, externalPath);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }

        [Theory]
        [InlineData("/filename", "external")]
        public void Export_WhenFileNotExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            // Act
            void Act() => _client.ExportFile(fileName, externalPath);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("/filename", "external")]
        public void Export_WhenExternalFileAlreadyExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            _fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            _externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(true);

            // Act
            void Act() => _client.ExportFile(fileName, externalPath);

            // Assert
            Assert.Throws<ExternalFileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("/some filename")]
        [InlineData("/some-filename")]
        [InlineData("/some-filename123")]
        [InlineData("/1.2.3")]
        [InlineData("/123_123")]
        [InlineData("/_")]
        [InlineData("/a")]
        public void FileExists_WithValidParameters_ShouldCallExists(string fileName)
        {
            // Act
            _client.FileExists(fileName);

            // Assert
            _fileRepositoryMock.Verify(r => r.Exists(fileName));
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
        public void FileExists_WithInvalidFileName_ShouldThrowException(string fileName)
        {
            // Act
            void Act() => _client.FileExists(fileName);

            // Assert
            Assert.Throws<InvalidNameException>(Act);
        }
    }
}