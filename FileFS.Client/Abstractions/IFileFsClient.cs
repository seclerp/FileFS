using System.Collections.Generic;
using FileFS.DataAccess.Entities;

namespace FileFS.Client.Abstractions
{
    public interface IFileFsClient
    {
        void Create(string fileName, byte[] contentBytes);

        void Update(string fileName, byte[] newContentBytes);

        byte[] ReadContent(string fileName);

        void Rename(string oldFilename, string newFilename);

        void Delete(string fileName);

        void Import(string externalPath, string fileName);

        void Export(string fileName, string externalPath);

        bool Exists(string fileName);

        IReadOnlyCollection<FileEntryInfo> ListFiles();

        void ForceOptimize();
    }
}