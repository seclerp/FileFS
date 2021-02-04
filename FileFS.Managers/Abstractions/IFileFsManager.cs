using System.Collections.Generic;
using FileFS.Managers.Models;

namespace FileFS.Managers.Abstractions
{
    public interface IFileFsManager
    {
        void Create(string fileName, byte[] contentBytes);

        byte[] Read(string fileName);

        void Rename(string oldFilename, string newFilename);

        void Delete(string fileName);

        bool Exists(string fileName);

        IReadOnlyCollection<EntryInfo> List();
    }
}