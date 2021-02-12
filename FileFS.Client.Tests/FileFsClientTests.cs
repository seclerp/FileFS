using System;
using System.IO;
using System.Text;
using FileFS.Client.Abstractions;
using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileFS.Tests.Shared.Comparers;
using Moq;
using Xunit;
using FileNotFoundException = FileFS.Client.Exceptions.FileNotFoundException;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Client.Tests
{
    public class FileFsClientTests
    {
        [Theory]
        [InlineData("some filename")]
        [InlineData("some-filename")]
        [InlineData("some-filename123")]
        [InlineData("1.2.3")]
        [InlineData("123_123")]
        [InlineData("_")]
        [InlineData("a")]
        public void CreateEmpty_WithValidParameters_ShouldCallCreate(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Create(fileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Create(It.IsAny<FileEntry>()));
        }

        [Theory]
        [InlineData("file$name")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void CreateEmpty_WhenFileAlreadyExists_ShouldThrowException(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Create(fileName);

            // Assert
            Assert.Throws<FileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("some filename", "")]
        [InlineData("some-filename", "123123123")]
        [InlineData("some-filename123", "123123123")]
        [InlineData("1.2.3", "asdasd")]
        [InlineData("123_123", "asdasd")]
        [InlineData("_", "asdasd")]
        [InlineData("a", "12312312")]
        public void Create_WithValidParameters_ShouldCallCreate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileRepositoryMock = new Mock<IFileRepository>();

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Create(fileName, dataBytes);

            // Assert
            fileRepositoryMock.Verify(r => r.Create(It.IsAny<FileEntry>()));
        }

        [Theory]
        [InlineData("file$name", "")]
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
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, dataBytes);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "")]
        public void Create_WhenFileAlreadyExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Create(fileName, dataBytes);

            // Assert
            Assert.Throws<FileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void Create_WithNullData_ShouldThrowException(string fileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, null);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("some filename", "")]
        [InlineData("some-filename", "123123123")]
        [InlineData("some-filename123", "123123123")]
        [InlineData("1.2.3", "asdasd")]
        [InlineData("123_123", "asdasd")]
        [InlineData("_", "asdasd")]
        [InlineData("a", "12312312")]
        public void CreateStreamed_WithValidParameters_ShouldCallCreate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            var fileRepositoryMock = new Mock<IFileRepository>();

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Create(fileName, stream, dataBytes.Length);

            // Assert
            fileRepositoryMock.Verify(r => r.Create(It.IsAny<StreamedFileEntry>()));
        }

        [Theory]
        [InlineData("file$name", "")]
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

            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "")]
        public void CreateStreamed_WhenFileAlreadyExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Create(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<FileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void CreateStreamed_WithNullDataStream_ShouldThrowException(string fileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, null, 0);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("some filename", "")]
        [InlineData("some-filename", "123123123")]
        [InlineData("some-filename123", "123123123")]
        [InlineData("1.2.3", "asdasd")]
        [InlineData("123_123", "asdasd")]
        [InlineData("_", "asdasd")]
        [InlineData("a", "12312312")]
        public void Update_WithValidParameters_ShouldCallUpdate(string fileName, string newData)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(newData);

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Update(fileName, dataBytes);

            // Assert
            fileRepositoryMock.Verify(r => r.Update(It.IsAny<FileEntry>()));
        }

        [Theory]
        [InlineData("file$name", "")]
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
            var client = CreateClient();

            // Act
            void Act() => client.Update(fileName, dataBytes);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "data")]
        public void Update_WhenFileNotExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => false);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Update(fileName, dataBytes);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void Update_WithNullData_ShouldThrowException(string fileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, null);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("some filename", "")]
        [InlineData("some-filename", "123123123")]
        [InlineData("some-filename123", "123123123")]
        [InlineData("1.2.3", "asdasd")]
        [InlineData("123_123", "asdasd")]
        [InlineData("_", "asdasd")]
        [InlineData("a", "12312312")]
        public void UpdateStreamed_WithValidParameters_ShouldCallUpdate(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Update(fileName, stream, dataBytes.Length);

            // Assert
            fileRepositoryMock.Verify(r => r.Update(It.IsAny<StreamedFileEntry>()));
        }

        [Theory]
        [InlineData("file$name", "")]
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

            var client = CreateClient();

            // Act
            void Act() => client.Update(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "data")]
        public void UpdateStreamed_WhenFileNotExists_ShouldThrowException(string fileName, string data)
        {
            // Arrange
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var stream = new MemoryStream(dataBytes);

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => false);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Update(fileName, stream, dataBytes.Length);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void UpdateStreamed_WithNullDataStream_ShouldThrowException(string fileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Create(fileName, null, 0);

            // Assert
            Assert.Throws<DataIsNullException>(Act);
        }

        [Theory]
        [InlineData("some filename")]
        [InlineData("some-filename")]
        [InlineData("some-filename123")]
        [InlineData("1.2.3")]
        [InlineData("123_123")]
        [InlineData("_")]
        [InlineData("a")]
        public void Read_WithValidParameters_ShouldCallRead(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Read(fileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Read(fileName));
        }

        [Theory]
        [InlineData("file$name")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Read(fileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("some filename")]
        [InlineData("some-filename")]
        [InlineData("some-filename123")]
        [InlineData("1.2.3")]
        [InlineData("123_123")]
        [InlineData("_")]
        [InlineData("a")]
        public void ReadStreamed_WithValidParameters_ShouldCallRead(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Read(fileName, new MemoryStream());

            // Assert
            fileRepositoryMock.Verify(r => r.Read(fileName, It.IsAny<Stream>()));
        }

        [Theory]
        [InlineData("file$name")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Read(fileName, new MemoryStream());

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void ReadStreamed_WithDestinationStreamNull_ShouldThrowException(string fileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Read(fileName, null);

            // Arrange
            Assert.Throws<ArgumentNonValidException>(Act);
        }

        [Theory]
        [InlineData("from", "to")]
        public void Rename_WithValidParameters_ShouldCallRename(string oldFileName, string newFileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Rename(oldFileName, newFileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Rename(oldFileName, newFileName));
        }

        [Theory]
        [InlineData("file$name", "valid")]
        [InlineData("", "valid")]
        [InlineData(null, "valid")]
        [InlineData("()9", "valid")]
        [InlineData("+++", "valid")]
        [InlineData("#sdfd", "valid")]
        [InlineData("\\/\\/asdasd", "valid")]
        [InlineData("!!!asdasd!!!", "valid")]
        [InlineData("[dapk_xantep]", "valid")]
        [InlineData("&lol&", "valid")]
        [InlineData("%%", "valid")]
        [InlineData("valid", "file$name")]
        [InlineData("valid", "")]
        [InlineData("valid", null)]
        [InlineData("valid", "()9")]
        [InlineData("valid", "+++")]
        [InlineData("valid", "#sdfd")]
        [InlineData("valid", "\\/\\/asdasd")]
        [InlineData("valid", "!!!asdasd!!!")]
        [InlineData("valid", "[dapk_xantep]")]
        [InlineData("valid", "&lol&")]
        [InlineData("valid", "%%")]
        [InlineData("432$$4", "%%")]
        public void Rename_WithOneOfNamesIsInvalid_ShouldThrowException(string oldFileName, string newFileName)
        {
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Rename(oldFileName, newFileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("some filename")]
        [InlineData("some-filename")]
        [InlineData("some-filename123")]
        [InlineData("1.2.3")]
        [InlineData("123_123")]
        [InlineData("_")]
        [InlineData("a")]
        public void Delete_WithValidParameters_ShouldDeleteFile(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => true);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Delete(fileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Delete(fileName));
        }

        [Theory]
        [InlineData("file$name")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Delete(fileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename")]
        public void Delete_WhenFileNotExists_ShouldThrowException(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(() => false);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Delete(fileName);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("some filename", "external")]
        [InlineData("some-filename", "external")]
        [InlineData("some-filename123", "external")]
        [InlineData("1.2.3", "external")]
        [InlineData("123_123", "external")]
        [InlineData("_", "external")]
        [InlineData("a", "external")]
        public void Import_WithValidParameters_ShouldImportFile(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(true);
            externalFileManagerMock
                .Setup(e => e.OpenReadStream(externalPath))
                .Returns(new MemoryStream());

            var client = CreateClient(fileRepositoryMock.Object, externalFileManagerMock.Object);

            // Act
            client.Import(externalPath, fileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Create(It.Is<StreamedFileEntry>(fileEntry => fileEntry.FileName == fileName)));
        }

        [Theory]
        [InlineData("file$name", "external")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Import(externalPath, fileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "external")]
        public void Import_WhenFileAlreadyExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Import(externalPath, fileName);

            // Assert
            Assert.Throws<FileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("filename", "external")]
        public void Import_WhenExternalFileNotExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(false);

            var client = CreateClient(fileRepositoryMock.Object, externalFileManagerMock.Object);

            // Act
            void Act() => client.Import(externalPath, fileName);

            // Assert
            Assert.Throws<ExternalFileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("some filename", "external")]
        [InlineData("some-filename", "external")]
        [InlineData("some-filename123", "external")]
        [InlineData("1.2.3", "external")]
        [InlineData("123_123", "external")]
        [InlineData("_", "external")]
        [InlineData("a", "external")]
        public void Export_WithValidParameters_ShouldExportFile(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(false);
            externalFileManagerMock
                .Setup(e => e.OpenWriteStream(externalPath))
                .Returns(new MemoryStream());

            var client = CreateClient(fileRepositoryMock.Object, externalFileManagerMock.Object);

            // Act
            client.Export(fileName, externalPath);

            // Assert
            fileRepositoryMock.Verify(r => r.Read(fileName, It.IsAny<Stream>()));
        }

        [Theory]
        [InlineData("file$name", "external")]
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
            var client = CreateClient();

            // Act
            void Act() => client.Export(fileName, externalPath);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Theory]
        [InlineData("filename", "external")]
        public void Export_WhenFileNotExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(false);
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            void Act() => client.Export(fileName, externalPath);

            // Assert
            Assert.Throws<FileNotFoundException>(Act);
        }

        [Theory]
        [InlineData("filename", "external")]
        public void Export_WhenExternalFileAlreadyExists_ShouldThrowException(string fileName, string externalPath)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.Exists(fileName))
                .Returns(true);

            var externalFileManagerMock = new Mock<IExternalFileManager>();
            externalFileManagerMock
                .Setup(e => e.Exists(externalPath))
                .Returns(true);

            var client = CreateClient(fileRepositoryMock.Object, externalFileManagerMock.Object);

            // Act
            void Act() => client.Export(fileName, externalPath);

            // Assert
            Assert.Throws<ExternalFileAlreadyExistsException>(Act);
        }

        [Theory]
        [InlineData("some filename")]
        [InlineData("some-filename")]
        [InlineData("some-filename123")]
        [InlineData("1.2.3")]
        [InlineData("123_123")]
        [InlineData("_")]
        [InlineData("a")]
        public void Exists_WithValidParameters_ShouldCallExists(string fileName)
        {
            // Arrange
            var fileRepositoryMock = new Mock<IFileRepository>();
            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            client.Exists(fileName);

            // Assert
            fileRepositoryMock.Verify(r => r.Exists(fileName));
        }

        [Theory]
        [InlineData("file$name")]
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
            // Arrange
            var client = CreateClient();

            // Act
            void Act() => client.Exists(fileName);

            // Assert
            Assert.Throws<InvalidFilenameException>(Act);
        }

        [Fact]
        public void ListFiles_WhenThereAreFiles_ShouldCallGetAllFilesInfo()
        {
            // Arrange
            var firstFileName = "hello";
            var firstFileData = Encoding.UTF8.GetBytes("123423423423423423423");

            var secondFileName = "world";
            var secondFileData = Encoding.UTF8.GetBytes("123");

            var expectedListFiles = new[]
            {
                new FileEntryInfo(firstFileName, firstFileData.Length, DateTime.UtcNow, DateTime.UtcNow),
                new FileEntryInfo(secondFileName, secondFileData.Length, DateTime.UtcNow, DateTime.UtcNow),
            };

            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.GetAllFilesInfo())
                .Returns(expectedListFiles);

            var client = CreateClient(fileRepositoryMock.Object);

            // Act
            var files = client.ListFiles();

            // Assert
            Assert.Equal(expectedListFiles, files, new FileEntryInfoEqualityComparer());
        }

        [Fact]
        public void ListFiles_WhenThereAreNoFiles_ShouldReturnEmptyCollection()
        {
            // Arrange
            var client = CreateClient();
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(r => r.GetAllFilesInfo())
                .Returns(Array.Empty<FileEntryInfo>());

            // Act
            var files = client.ListFiles();

            // Assert
            Assert.Empty(files);
        }

        private static IFileFsClient CreateClient(
            IFileRepository repository = null,
            IExternalFileManager externalFileManager = null,
            IStorageOptimizer storageOptimizer = null)
        {
            repository ??= new Mock<IFileRepository>().Object;
            externalFileManager ??= new Mock<IExternalFileManager>().Object;
            storageOptimizer ??= new Mock<IStorageOptimizer>().Object;

            var transactionWrapper = new Mock<ITransactionWrapper>();
            transactionWrapper.Setup(t => t.BeginTransaction());
            transactionWrapper.Setup(t => t.EndTransaction());

            var client = new FileFsClient(repository, externalFileManager, storageOptimizer, transactionWrapper.Object);
            return client;
        }
    }
}