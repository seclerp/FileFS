using System.Collections.Generic;
using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFileDescriptorRepository
    {
        StorageItem<FileDescriptor> Read(int offset);

        IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll();

        void Write(StorageItem<FileDescriptor> item);

        StorageItem<FileDescriptor> Find(string fileName);

        bool Exists(string fileName);
    }
}