using System.Collections.Generic;
using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstractions that represents file descriptor repository.
    /// </summary>
    public interface IEntryDescriptorRepository
    {
        /// <summary>
        /// Reads file descriptor from FileFS storage.
        /// </summary>
        /// <param name="cursor">Cursor in memory.</param>
        /// <returns>Storage item that represents file descriptor with its cursor in memory.</returns>
        StorageItem<EntryDescriptor> Read(Cursor cursor);

        /// <summary>
        /// Returns all entry descriptors stored in FileFS storage.
        /// </summary>
        /// <returns>All file descriptors storage items stored in FileFS storage.</returns>
        IReadOnlyCollection<StorageItem<EntryDescriptor>> ReadAll();

        /// <summary>
        /// Returns children entry descriptors of given entry stored in FileFS storage.
        /// </summary>
        /// <param name="entryName">Name of an entry.</param>
        /// <returns>All file descriptors storage items stored in FileFS storage.</returns>
        IReadOnlyCollection<StorageItem<EntryDescriptor>> ReadChildren(string entryName);

        /// <summary>
        /// Writes entry descriptor into FileFS storage.
        /// </summary>
        /// <param name="item">Storage item that represents file descriptor with its cursor in memory.</param>
        void Write(StorageItem<EntryDescriptor> item);

        /// <summary>
        /// Finds entry descriptor by file name.
        /// </summary>
        /// <param name="entryName">Name of the file.</param>
        /// <returns>Storage item that represents file descriptor with its cursor in memory.</returns>
        StorageItem<EntryDescriptor> Find(string entryName);

        /// <summary>
        /// Returns true if file descriptor for given filename exists, otherwise false.
        /// </summary>
        /// <param name="entryName">Name of a file to check.</param>
        /// <returns>True if file descriptor for given filename exists, otherwise false.</returns>
        bool Exists(string entryName);

        bool TryFind(string entryName, out StorageItem<EntryDescriptor> item);
    }
}