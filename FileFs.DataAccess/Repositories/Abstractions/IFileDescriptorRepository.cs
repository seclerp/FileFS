using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFileDescriptorRepository
    {
        FileDescriptor Read(int offset);

        void Write(FileDescriptor descriptor, int offset);
    }
}