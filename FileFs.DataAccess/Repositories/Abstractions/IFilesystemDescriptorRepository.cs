using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFilesystemDescriptorRepository
    {
        FilesystemDescriptor Read();

        void Write(FilesystemDescriptor descriptor);
    }
}