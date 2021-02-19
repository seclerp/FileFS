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
using FileFS.DataAccess.Entities;
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

            _directoryRepository.Create(name);

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
                throw new DirectoryNotFoundException(fileName);
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

            _fileRepository.Read(fileName, destinationStream);

            _transactionWrapper.EndTransaction();
        }

        /// <inheritdoc />
        public void MoveDirectory(string currentDirectoryName, string newDirectoryName)
        {
            throw new NotImplementedException();
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

            _fileRepository.Rename(currentName, newName);

            _transactionWrapper.EndTransaction();
        }

        public void DeleteDirectory(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidNameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        public void DeleteFile(string fileName)
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

            _fileRepository.Delete(fileName);

            _transactionWrapper.EndTransaction();
        }

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
            if (!ExistsInternal(newParentName))
            {
                throw new EntryNotFoundException(from);
            }

            _entryRepository.Move(from, to);

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
        public IEnumerable<FileFsEntryInfo> ListFiles()
        {
            return _fileRepository
                .GetAllFilesInfo()
                .ToArray();
        }

        /// <inheritdoc />
        public bool IsDirectory(string name)
        {
            _transactionWrapper.BeginTransaction();

            if (!NameValid(name))
            {
                throw new InvalidNameException(name);
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
            _fileRepository.Read(name, destinationStream);
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
    }
}