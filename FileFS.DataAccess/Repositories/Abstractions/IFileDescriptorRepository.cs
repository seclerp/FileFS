using System.Collections.Generic;
using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstractions that represents file descriptor repository.
    /// </summary>
    public interface IFileDescriptorRepository
    {
        /// <summary>
        /// Reads file descriptor from FileFS storage.
        /// </summary>
        /// <param name="cursor">Cursor in memory.</param>
        /// <returns>Storage item that represents file descriptor with its cursor in memory.</returns>
        StorageItem<FileDescriptor> Read(Cursor cursor);

        /// <summary>
        /// Returns all file descriptors stored in FileFS storage.
        /// </summary>
        /// <returns>All file descriptors storage items stored in FileFS storage.</returns>
        IReadOnlyCollection<StorageItem<FileDescriptor>> ReadAll();

        /// <summary>
        /// Writes file descriptor into FileFS storage.
        /// </summary>
        /// <param name="item">Storage item that represents file descriptor with its cursor in memory.</param>
        void Write(StorageItem<FileDescriptor> item);

        /// <summary>
        /// Finds file descriptor by file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Storage item that represents file descriptor with its cursor in memory.</returns>
        StorageItem<FileDescriptor> Find(string fileName);

        /// <summary>
        /// Returns true if file descriptor for given filename exists, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a file to check.</param>
        /// <returns>True if file descriptor for given filename exists, otherwise false.</returns>
        bool Exists(string fileName);
    }
}