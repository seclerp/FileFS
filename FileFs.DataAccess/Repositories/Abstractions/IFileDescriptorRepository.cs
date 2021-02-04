using System.Collections.Generic;
using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFileDescriptorRepository
    {
        FileDescriptor Read(int offset);

        IReadOnlyCollection<FileDescriptor> ReadAll();

        void Write(FileDescriptor descriptor, int offset);
    }
}