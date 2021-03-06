﻿using System.IO;
using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstraction that represents file repository.
    /// </summary>
    public interface IFileRepository : IEntryRepository
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
        /// Copies existing file to a new destination.
        /// </summary>
        /// <param name="fileNameFrom">Name of an existing file to copy.</param>
        /// <param name="fileNameTo">Destination name of a file to copy to.</param>
        void Copy(string fileNameFrom, string fileNameTo);

        /// <summary>
        /// Reads file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <returns>File entry that represents existing file's name and data.</returns>
        FileEntry Read(string fileName);

        /// <summary>
        /// Reads file data with given filename into destination stream.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <param name="destinationStream">Destination stream of data.</param>
        void ReadData(string fileName, Stream destinationStream);
    }
}