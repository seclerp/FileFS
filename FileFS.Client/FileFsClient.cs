using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileFS.Client.Abstractions;
using FileFS.Client.Constants;
using FileFS.Client.Exceptions;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Repositories.Abstractions;

namespace FileFS.Client
{
    public class FileFsClient : IFileFsClient
    {
        private readonly IFileRepository _fileRepository;
        private readonly IExternalFileManager _externalFileManager;
        private readonly IStorageOptimizer _optimizer;

        public FileFsClient(
            IFileRepository fileRepository,
            IExternalFileManager externalFileManager,
            IStorageOptimizer optimizer)
        {
            _fileRepository = fileRepository;
            _externalFileManager = externalFileManager;
            _optimizer = optimizer;
        }

        public void Create(string fileName, byte[] contentBytes)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (contentBytes.Length is 0)
            {
                throw new EmptyContentException(fileName);
            }

            _fileRepository.Create(new FileEntry(fileName, contentBytes));
        }

        public void Update(string fileName, byte[] newContentBytes)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (newContentBytes.Length is 0)
            {
                throw new EmptyContentException(fileName);
            }

            _fileRepository.Update(new FileEntry(fileName, newContentBytes));
        }

        public byte[] ReadContent(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            return _fileRepository.Read(fileName).Content;
        }

        public void Rename(string oldFilename, string newFilename)
        {
            if (!IsValidFilename(oldFilename))
            {
                throw new InvalidFilenameException(oldFilename);
            }

            if (!IsValidFilename(newFilename))
            {
                throw new InvalidFilenameException(newFilename);
            }
        }

        public void Delete(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            _fileRepository.Delete(fileName);
        }

        public void Import(string externalPath, string fileName)
        {
            if (Exists(fileName))
            {
                throw new FileAlreadyExistsException(fileName);
            }

            if (!File.Exists(externalPath))
            {
                throw new ExternalFileNotFoundException(externalPath);
            }

            var contentBytes = _externalFileManager.Read(externalPath);

            Create(fileName, contentBytes);
        }

        public void Export(string fileName, string externalPath)
        {
            if (!Exists(fileName))
            {
                throw new Exceptions.FileNotFoundException(fileName);
            }

            if (File.Exists(externalPath))
            {
                throw new ExternalFileAlreadyExistsException(externalPath);
            }

            var contentBytes = ReadContent(fileName);

            _externalFileManager.Write(externalPath, contentBytes);
        }

        public bool Exists(string fileName)
        {
            if (!IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(fileName);
            }

            return _fileRepository.Exists(fileName);
        }

        public IReadOnlyCollection<FileEntryInfo> ListFiles()
        {
            return _fileRepository
                .GetAllFilesInfo()
                .ToArray();
        }

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