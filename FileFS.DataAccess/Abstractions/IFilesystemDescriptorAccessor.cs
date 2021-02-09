using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction that represent accessor for filesystem descriptor.
    /// </summary>
    public interface IFilesystemDescriptorAccessor
    {
        /// <summary>
        /// Gets current filesystem descriptor value.
        /// </summary>
        FilesystemDescriptor Value { get; }

        /// <summary>
        /// Updates filesystem descriptor with new value.
        /// </summary>
        /// <param name="descriptor">New filesystem descriptor value.</param>
        void Update(FilesystemDescriptor descriptor);
    }
}