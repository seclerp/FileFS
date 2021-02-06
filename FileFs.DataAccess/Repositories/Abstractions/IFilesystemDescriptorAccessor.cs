using FileFs.DataAccess.Entities;

namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFilesystemDescriptorAccessor
    {
        FilesystemDescriptor Value { get; }

        void Update(FilesystemDescriptor descriptor);
    }
}