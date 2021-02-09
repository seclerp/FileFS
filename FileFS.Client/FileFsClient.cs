using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileFS.Client.Abstractions;
using FileFS.Client.Constants;
using FileFS.Client.Exceptions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Exceptions;
using FileFS.DataAccess.Memory.Abstractions;
using FileFS.DataAccess.Repositories.Abstractions;
using FileNotFoundException = FileFS.Client.Exceptions.FileNotFoundException;

namespace FileFS.Client
{
    /// <summary>
    /// Implementation of client for working with FileFS storage.
    /// </summary>
    public class FileFsClient : IFileFsClient
    {
        private readonly IFileRepository _fileRepository;
        private readonly IExternalFileManager _externalFileManager;
        private readonly IStorageOptimizer _optimizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFsClient"/> class.
        /// </summary>
        /// <param name="fileRepository">File repository instance.</param>
        /// <param name="externalFileManager">External file manager instance.</param>
        /// <param name="optimizer">Optimizer instance.</param>
        public FileFsClient(
            IFileRepository fileRepository,
            IExternalFileManager externalFileManager,
            IStorageOptimizer optimizer)
        {
            _fileRepository = fileRepository;
            _externalFileManager = externalFileManager;
            _optimizer = optimizer;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="EmptyDataException">Throws if data is empty.</exception>
        public void Create(string fileName, byte[] data)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (data is null || data.Length is 0)
            {
                throw new EmptyDataException(fileName);
            }

            _fileRepository.Create(new FileEntry(fileName, data));
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="EmptyDataException">Throws if data is empty.</exception>
        public void Create(string fileName, Stream sourceStream, int length)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (sourceStream is null || sourceStream.Length is 0)
            {
                throw new EmptyDataException(fileName);
            }

            _fileRepository.Create(new StreamedFileEntry(fileName, sourceStream, length));
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="EmptyDataException">Throws if data is empty.</exception>
        public void Update(string fileName, byte[] newData)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (newData is null || newData.Length is 0)
            {
                throw new EmptyDataException(fileName);
            }

            _fileRepository.Update(new FileEntry(fileName, newData));
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="EmptyDataException">Throws if data is empty.</exception>
        public void Update(string fileName, Stream sourceStream, int length)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (sourceStream is null || sourceStream.Length is 0)
            {
                throw new EmptyDataException(fileName);
            }

            _fileRepository.Update(new StreamedFileEntry(fileName, sourceStream, length));
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        public byte[] Read(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            return _fileRepository.Read(fileName).Data;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="ArgumentNonValidException">Throws when destinationStream is null.</exception>
        public void Read(string fileName, Stream destinationStream)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (destinationStream is null)
            {
                throw new ArgumentNonValidException($"Argument cannot be null: {nameof(destinationStream)}");
            }

            _fileRepository.Read(fileName, destinationStream);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if current or new filename is invalid.</exception>
        public void Rename(string currentFilename, string newFilename)
        {
            if (!IsValidFilename(currentFilename))
            {
                throw new InvalidFilenameException(currentFilename);
            }

            if (!IsValidFilename(newFilename))
            {
                throw new InvalidFilenameException(newFilename);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        public void Delete(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            _fileRepository.Delete(fileName);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileAlreadyExistsException">Throws if file already exists.</exception>
        /// <exception cref="ExternalFileNotFoundException">Throws if external file was not found.</exception>
        public void Import(string externalPath, string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (!_externalFileManager.Exists(externalPath))
            {
                throw new ExternalFileNotFoundException(externalPath);
            }

            using var externalFileStream = _externalFileManager.OpenReadStream(externalPath);

            Create(fileName, externalFileStream, (int)externalFileStream.Length);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        /// <exception cref="FileNotFoundException">Throws if file not found.</exception>
        /// <exception cref="ExternalFileAlreadyExistsException">Throws if external file already exists.</exception>
        public void Export(string fileName, string externalPath)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            if (_externalFileManager.Exists(externalPath))
            {
                throw new ExternalFileAlreadyExistsException(externalPath);
            }

            using var externalFileStream = _externalFileManager.OpenWriteStream(externalPath);

            Read(fileName, externalFileStream);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidFilenameException">Throws if filename is invalid.</exception>
        public bool Exists(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            return _fileRepository.Exists(fileName);
        }

        /// <inheritdoc />
        public IEnumerable<FileEntryInfo> ListFiles()
        {
            return _fileRepository
                .GetAllFilesInfo()
                .ToArray();
        }

        /// <inheritdoc />
        public void ForceOptimize()
        {
            _optimizer.Optimize();
        }

        private static bool IsValidFilename(string fileName)
        {
            return Regex.IsMatch(fileName, PatternMatching.ValidFilename);
        }
    }
}