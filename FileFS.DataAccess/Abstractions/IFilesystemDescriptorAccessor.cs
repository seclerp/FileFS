using System;
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
        /// This operation is thread safe.
        /// </summary>
        FilesystemDescriptor Value { get; }

        /// <summary>
        /// Updates filesystem descriptor with new data using relative data updaters.
        /// This operation is thread safe.
        /// </summary>
        /// <param name="filesDataLengthUpdater">FilesDataLength updater.</param>
        /// <param name="entryDescriptorsCountUpdater">EntryDescriptorsCount updater.</param>
        /// <param name="entryDescriptorLengthUpdater">EntryDescriptorLength updater.</param>
        /// <returns>Updated filesystem descriptor instance.</returns>
        FilesystemDescriptor Update(
            Func<int, int> filesDataLengthUpdater = null,
            Func<int, int> entryDescriptorsCountUpdater = null,
            Func<int, int> entryDescriptorLengthUpdater = null);
    }
}