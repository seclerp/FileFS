using System.Collections.Generic;
using System.IO;
using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstraction that represents file repository.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Creates new file in FileFS storage.
        /// </summary>
        /// <param name="file">File entry that represents new file's name and data.</param>
        void Create(FileEntry file);

        /// <summary>
        /// Creates new file in FileFS storage.
        /// </summary>
        /// <param name="streamedFile">Streamed file entry that represents new file's name and data.</param>
        void Create(StreamedFileEntry streamedFile);

        /// <summary>
        /// Updates data of existing file.
        /// </summary>
        /// <param name="file">File entry that represents existing file's name and new data.</param>
        void Update(FileEntry file);

        /// <summary>
        /// Updates data of existing file.
        /// </summary>
        /// <param name="streamedFile">Streamed file entry that represents existing file's name and new data.</param>
        void Update(StreamedFileEntry streamedFile);

        /// <summary>
        /// Reads file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <returns>File entry that represents existing file's name and data.</returns>
        FileEntry Read(string fileName);

        /// <summary>
        /// Reads file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <param name="destinationStream">Destination stream of data.</param>
        void Read(string fileName, Stream destinationStream);

        /// <summary>
        /// Renames file with given filename.
        /// </summary>
        /// <param name="currentFilename">Current name of a file to rename.</param>
        /// <param name="newFilename">New name of a file to rename.</param>
        void Rename(string currentFilename, string newFilename);

        /// <summary>
        /// Deletes file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file to delete.</param>
        void Delete(string fileName);

        /// <summary>
        /// Returns true if file with given filename exists, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a file to check.</param>
        /// <returns>True if file with given filename exists, otherwise false.</returns>
        bool Exists(string fileName);

        /// <summary>
        /// Returns all files details.
        /// </summary>
        /// <returns>All files details.</returns>
        IEnumerable<FileEntryInfo> GetAllFilesInfo();
    }
}