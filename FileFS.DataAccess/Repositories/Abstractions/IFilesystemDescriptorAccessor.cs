using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    public interface IFilesystemDescriptorAccessor
    {
        FilesystemDescriptor Value { get; }

        void Update(FilesystemDescriptor descriptor);
    }
}