using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileFS.Client.Abstractions;
using FileFS.Client.Constants;
using FileFS.Client.Exceptions;
using FileFS.Client.Transactions.Abstractions;
using FileFS.DataAccess.Allocation.Abstractions;
using FileFS.DataAccess.Constants;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Entities.Enums;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Extensions;
using FileFS.DataAccess.Repositories.Abstractions;
using DirectoryNotFoundException = FileFS.Client.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = FileFS.Client.Exceptions.FileNotFoundException;

namespace FileFS.Client
{
    /// <summary>
    /// Implementation of client for working with FileFS storage.
    /// </summary>
    public class FileFsClient : IFileFsClient, IDisposable
    {
        private readonly IFileRepository _fileRepository;
        private readonly IDirectoryRepository _directoryRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly IExternalFileManager _externalFileManager;
        private readonly IStorageOptimizer _optimizer;
        private readonly ITransactionWrapper _transactionWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFsClient"/> class.
        /// </summary>
        /// <param name="fileRepository">File repository instance.</param>
        /// <param name="directoryRepository">Directory repository instance.</param>
        /// <param name="entryRepository">Entry repository instance.</param>
        /// <param name="externalFileManager">External file manager instance.</param>
        /// <param name="optimizer">Optimizer instance.</param>
        /// <param name="transactionWrapper">Transaction wrapper instance.</param>
        public FileFsClient(
            IFileRepository fileRepository,
            IDirectoryRepository directoryRepository,
            IEntryRepository entryRepository,
            IExternalFileManager externalFileManager,
            IStorageOptimizer optimizer,
            ITransactionWrapper transactionWrapper)
        {
            _fileRepository = fileRepository;
            _directoryRepository = directoryRepository;
            _entryRepository = entryRepository;
            _externalFileManager = externalFileManager;
            _optimizer = optimizer;
            _transactionWrapper = transactionWrapper;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if name is invalid.</exception>
        /// <exception cref="EntryAlreadyExistsException">Throws if file already exists.</exception>
        public void CreateDirectory(string name)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(name))
            {
                throw new InvalidNameException(name);
            }

            if (ExistsInternal(name))
            {
                throw new EntryAlreadyExistsException(name);
            }

            var parentDirectoryName = name.GetParentFullName();
            if (!DirectoryExistsInternal(parentDirectoryName))
            {
                throw new DirectoryNotFoundException(name);
            }

            var directoryEntry = CreateDirectoryEntry(name);
            _directoryRepository.Create(directoryEntry);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="EntryAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="DirectoryNotFoundException">Throws if parent directory is not found.</exception>
        public void CreateFile(string fileName)
        {
            CreateFile(fileName, Array.Empty<byte>());
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="EntryAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="DirectoryNotFoundException">Throws if parent directory is not found.</exception>
        /// <exception cref="DataIsNullException">Throws if data is empty.</exception>
        public void CreateFile(string fileName, byte[] data)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (ExistsInternal(fileName))
            {
                throw new EntryAlreadyExistsException(fileName);
            }

            var parentDirectoryName = fileName.GetParentFullName();
            if (!DirectoryExistsInternal(parentDirectoryName))
            {
                throw new DirectoryNotFoundException(parentDirectoryName);
            }

            if (data is null)
            {
                throw new DataIsNullException(fileName);
            }

            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            _fileRepository.Create(new FileEntry(fileName, parentDirectory.Id, data));

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="EntryAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="DirectoryNotFoundException">Throws if parent directory is not found.</exception>
        /// <exception cref="DataIsNullException">Throws if data is empty.</exception>
        public void CreateFile(string fileName, Stream sourceStream, int length)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (ExistsInternal(fileName))
            {
                throw new EntryAlreadyExistsException(fileName);
            }

            var parentDirectoryName = fileName.GetParentFullName();
            if (!DirectoryExistsInternal(parentDirectoryName))
            {
                throw new DirectoryNotFoundException(fileName);
            }

            if (sourceStream is null)
            {
                throw new DataIsNullException(fileName);
            }

            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            _fileRepository.Create(new StreamedFileEntry(fileName, parentDirectory.Id, sourceStream, length));

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="DataIsNullException">Throws if data is empty.</exception>
        public void Update(string fileName, byte[] newData)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (!FileExistsInternal(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (newData is null)
            {
                throw new DataIsNullException(fileName);
            }

            var parentDirectoryName = fileName.GetParentFullName();
            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            _fileRepository.Update(new FileEntry(fileName, parentDirectory.Id, newData));

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="DataIsNullException">Throws if data is empty.</exception>
        public void Update(string fileName, Stream sourceStream, int length)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (!FileExistsInternal(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (sourceStream is null)
            {
                throw new DataIsNullException(fileName);
            }

            var parentDirectoryName = fileName.GetParentFullName();
            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            _fileRepository.Update(new StreamedFileEntry(fileName, parentDirectory.Id, sourceStream, length));

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        public byte[] Read(string fileName)
        {
            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (!FileExistsInternal(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            return _fileRepository.Read(fileName).Data;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="ArgumentNonValidException">Throws when destinationStream is null.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        public void Read(string fileName, Stream destinationStream)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (!FileExistsInternal(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (destinationStream is null)
            {
                throw new ArgumentNonValidException($"Argument cannot be null: {nameof(destinationStream)}");
            }

            _fileRepository.ReadData(fileName, destinationStream);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if current or new name is invalid.</exception>
        /// <exception cref="EntryNotFoundException">Throws if entry not found.</exception>
        public void Rename(string currentName, string newName)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(currentName))
            {
                throw new InvalidNameException(currentName);
            }

            if (!NameValid(newName))
            {
                throw new InvalidNameException(newName);
            }

            if (!ExistsInternal(currentName))
            {
                throw new EntryNotFoundException(currentName);
            }

            if (ExistsInternal(newName))
            {
                throw new EntryAlreadyExistsException(newName);
            }

            var currentParentName = currentName.GetParentFullName();
            var newParentName = newName.GetParentFullName();

            if (currentParentName != newParentName)
            {
                throw new ArgumentNonValidException($"New name of an entry should be inside same directory as current name, expected '{currentParentName}', got '{newParentName}'");
            }

            if (currentName == PathConstants.RootDirectoryName)
            {
                throw new OperationIsInvalid("Rename of root directory is not allowed");
            }

            _entryRepository.Rename(currentName, newName);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        public void Move(string from, string to)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(from))
            {
                throw new InvalidNameException(from);
            }

            if (!NameValid(to))
            {
                throw new InvalidNameException(to);
            }

            if (!ExistsInternal(from))
            {
                throw new EntryNotFoundException(from);
            }

            if (ExistsInternal(to))
            {
                throw new EntryAlreadyExistsException(to);
            }

            var newParentName = to.GetParentFullName();
            if (!DirectoryExistsInternal(newParentName))
            {
                throw new DirectoryNotFoundException(newParentName);
            }

            if (from == PathConstants.RootDirectoryName)
            {
                throw new OperationIsInvalid("Move of root directory is not allowed");
            }

            if (from.IsParentTo(to))
            {
                throw new OperationIsInvalid("Move of parent entry inside it child is not allowed");
            }

            _entryRepository.Move(from, to);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        public void Copy(string from, string to)
        {
            if (!NameValid(from))
            {
                throw new InvalidNameException(from);
            }

            if (!NameValid(to))
            {
                throw new InvalidNameException(to);
            }

            if (!ExistsInternal(from))
            {
                throw new EntryNotFoundException(from);
            }

            if (ExistsInternal(to))
            {
                throw new EntryAlreadyExistsException(to);
            }

            var newParentName = to.GetParentFullName();
            if (!DirectoryExistsInternal(newParentName))
            {
                throw new DirectoryNotFoundException(newParentName);
            }

            if (from == PathConstants.RootDirectoryName)
            {
                throw new OperationIsInvalid("Copy of root directory is not allowed");
            }

            if (from.IsParentTo(to))
            {
                throw new OperationIsInvalid("Copy of parent entry inside it child is not allowed");
            }

            CopyInternal(from, to);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        public void Delete(string name)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(name))
            {
                throw new InvalidNameException(name);
            }

            if (!ExistsInternal(name))
            {
                throw new EntryNotFoundException(name);
            }

            if (name == PathConstants.RootDirectoryName)
            {
                throw new ArgumentNonValidException("Delete of root directory is not allowed");
            }

            DeleteInternal(name);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="EntryAlreadyExistsException">Throws if entry already exists.</exception>
        /// <exception cref="ExternalFileNotFoundException">Throws if external file was not found.</exception>
        public void ImportFile(string externalPath, string fileName)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (ExistsInternal(fileName))
            {
                throw new EntryAlreadyExistsException(fileName);
            }

            if (!_externalFileManager.Exists(externalPath))
            {
                throw new ExternalFileNotFoundException(externalPath);
            }

            using var externalFileStream = _externalFileManager.OpenReadStream(externalPath);
            CreateFileInternal(fileName, externalFileStream, (int)externalFileStream.Length);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="ExternalFileAlreadyExistsException">Throws if external file already exists.</exception>
        public void ExportFile(string fileName, string externalPath)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            if (!FileExistsInternal(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (_externalFileManager.Exists(externalPath))
            {
                throw new ExternalFileAlreadyExistsException(externalPath);
            }

            using var externalFileStream = _externalFileManager.OpenWriteStream(externalPath);
            ReadInternal(fileName, externalFileStream);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        public bool FileExists(string fileName)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            var exists = FileExistsInternal(fileName);

            _transactionWrapper.EndTransaction();

            return exists;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if name is invalid.</exception>
        public bool DirectoryExists(string fileName)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(fileName))
            {
                throw new InvalidNameException(fileName);
            }

            var exists = DirectoryExistsInternal(fileName);

            _transactionWrapper.EndTransaction();

            return exists;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if name is invalid.</exception>
        public bool Exists(string name)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(name))
            {
                throw new InvalidNameException(name);
            }

            var exists = ExistsInternal(name);

            _transactionWrapper.EndTransaction();

            return exists;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if name is invalid.</exception>
        /// <exception cref="DirectoryNotFoundException">Throws if directory is not found.</exception>
        public IEnumerable<FileFsEntryInfo> GetEntries(string directoryName)
        {
            if (!NameValid(directoryName))
            {
                throw new InvalidNameException(directoryName);
            }

            if (!DirectoryExistsInternal(directoryName))
            {
                throw new DirectoryNotFoundException(directoryName);
            }

            return _entryRepository
                .GetEntriesInfo(directoryName)
                .Where(entryInfo => entryInfo.EntryName != PathConstants.RootDirectoryName);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if name is invalid.</exception>
        /// <exception cref="EntryNotFoundException">If there is no entry with such name.</exception>
        public bool IsDirectory(string name)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(name))
            {
                throw new InvalidNameException(name);
            }

            if (!ExistsInternal(name))
            {
                throw new EntryNotFoundException(name);
            }

            var result = _directoryRepository.Exists(name);

            _transactionWrapper.EndTransaction();

            return result;
        }

        /// <inheritdoc />
        public int ForceOptimize()
        {
            _transactionWrapper.BeginTransaction();
            var bytesOptimized = _optimizer.Optimize();
            _transactionWrapper.EndTransaction();

            return bytesOptimized;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _transactionWrapper?.Dispose();
        }

        private static bool NameValid(string fileName)
        {
            return fileName is { } && Regex.IsMatch(fileName, PatternMatchingConstants.ValidFilename);
        }

        private void CreateFileInternal(string name, Stream sourceStream, int length)
        {
            var parentDirectoryName = name.GetParentFullName();
            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            _fileRepository.Create(new StreamedFileEntry(name, parentDirectory.Id, sourceStream, length));
        }

        private void ReadInternal(string name, Stream destinationStream)
        {
            _fileRepository.ReadData(name, destinationStream);
        }

        private void CopyInternal(string from, string to)
        {
            if (DirectoryExistsInternal(from))
            {
                CopyDirectoryInternal(from, to);
            }
            else
            {
                CopyFileInternal(from, to);
            }
        }

        private void CopyFileInternal(string from, string to)
        {
            _fileRepository.Copy(from, to);
        }

        private void CopyDirectoryInternal(string from, string to)
        {
            var directoryEntry = CreateDirectoryEntry(to);
            _directoryRepository.Create(directoryEntry);

            var entriesInfo = _entryRepository.GetEntriesInfo(from);
            var directoriesInfo = entriesInfo.Where(entryInfo => entryInfo.EntryType is EntryType.Directory);
            var filesInfo = entriesInfo.Where(entryInfo => entryInfo.EntryType is EntryType.File);

            // Copy files firstly
            foreach (var fileInfo in filesInfo)
            {
                var fileShortName = fileInfo.EntryName.GetShortName();
                var newFileName = to.CombineWith(fileShortName);

                CopyFileInternal(fileInfo.EntryName, newFileName);
            }

            // Then - directories using recursion
            foreach (var directoryInfo in directoriesInfo)
            {
                var directoryShortName = directoryInfo.EntryName.GetShortName();
                var newDirectoryName = to.CombineWith(directoryShortName);

                CopyDirectoryInternal(directoryInfo.EntryName, newDirectoryName);
            }
        }

        private void DeleteInternal(string name)
        {
            if (DirectoryExistsInternal(name))
            {
                DeleteDirectoryInternal(name);
            }
            else
            {
                DeleteFileInternal(name);
            }
        }

        private void DeleteFileInternal(string name)
        {
            _entryRepository.Delete(name);
        }

        private void DeleteDirectoryInternal(string name)
        {
            var entriesInfo = _entryRepository.GetEntriesInfo(name);
            var directoriesInfo = entriesInfo.Where(entryInfo => entryInfo.EntryType is EntryType.Directory);
            var filesInfo = entriesInfo.Where(entryInfo => entryInfo.EntryType is EntryType.File);

            // Delete files firstly
            foreach (var fileInfo in filesInfo)
            {
                DeleteFileInternal(fileInfo.EntryName);
            }

            // Then - directories using recursion
            foreach (var directoryInfo in directoriesInfo)
            {
                DeleteDirectoryInternal(directoryInfo.EntryName);
            }

            // We should delete directory itself only after all descriptors inside this directory recursively would be deleted
            _directoryRepository.Delete(name);
        }

        private bool FileExistsInternal(string name)
        {
            var exists = _fileRepository.Exists(name);

            return exists;
        }

        private bool DirectoryExistsInternal(string name)
        {
            var exists = _directoryRepository.Exists(name);

            return exists;
        }

        private bool ExistsInternal(string name)
        {
            var exists = _entryRepository.Exists(name);

            return exists;
        }

        private DirectoryEntry CreateDirectoryEntry(string name)
        {
            var parentDirectoryName = name.GetParentFullName();
            var parentDirectory = _directoryRepository.Find(parentDirectoryName);
            var directoryEntry = new DirectoryEntry(Guid.NewGuid(), name, parentDirectory.Id);

            return directoryEntry;
        }
    }
}