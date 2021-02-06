using System.Collections.Generic;
using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFileRepository
    {
        void Create(FileEntry file);

        void Update(FileEntry file);

        FileEntry Read(string fileName);

        void Rename(string oldFilename, string newFilename);

        void Delete(string fileName);

        bool Exists(string fileName);

        IReadOnlyCollection<FileEntryInfo> GetAllFilesInfo();
    }
}