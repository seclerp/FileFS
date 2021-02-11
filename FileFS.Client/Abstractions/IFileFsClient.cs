﻿using System.Collections.Generic;
using System.IO;
using FileFS.DataAccess.Entities;

namespace FileFS.Client.Abstractions
{
    /// <summary>
    /// Abstraction that represents client for working with FileFS storage.
    /// </summary>
    public interface IFileFsClient
    {
        /// <summary>
        /// Creates new file with empty data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        void Create(string fileName);

        /// <summary>
        /// Creates new file with given data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        /// <param name="data">Data bytes.</param>
        void Create(string fileName, byte[] data);

        /// <summary>
        /// Creates new file with given data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        /// <param name="sourceStream">Source stream of bytes.</param>
        /// <param name="length">Length of data to be read in bytes.</param>
        void Create(string fileName, Stream sourceStream, int length);

        /// <summary>
        /// Updates existing file using new data.
        /// </summary>
        /// <param name="fileName">Name of a file to update.</param>
        /// <param name="newData">New data bytes.</param>
        void Update(string fileName, byte[] newData);

        /// <summary>
        /// Updates existing file using new data.
        /// </summary>
        /// <param name="fileName">Name of a file to update.</param>
        /// <param name="sourceStream">Source stream of bytes.</param>
        /// <param name="length">Length of data to be read in bytes.</param>
        void Update(string fileName, Stream sourceStream, int length);

        /// <summary>
        /// Reads data of existing file.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <returns>File's data bytes.</returns>
        byte[] Read(string fileName);

        /// <summary>
        /// Reads data of existing file.
        /// </summary>
        /// <param name="fileName">Name of a file to read.</param>
        /// <param name="destinationStream">Destination stream of bytes.</param>
        void Read(string fileName, Stream destinationStream);

        /// <summary>
        /// Change name of existing file.
        /// </summary>
        /// <param name="currentFilename">Current name of file..</param>
        /// <param name="newFilename">New name of a file.</param>
        void Rename(string currentFilename, string newFilename);

        /// <summary>
        /// Deletes existing file.
        /// </summary>
        /// <param name="fileName">Name of a file to delete.</param>
        void Delete(string fileName);

        /// <summary>
        /// Imports external file into FileFS storage.
        /// </summary>
        /// <param name="externalPath">Path to external file to import.</param>
        /// <param name="fileName">Name of a new file in FileFS storage.</param>
        void Import(string externalPath, string fileName);

        /// <summary>
        /// Exports file from FileFS storage to new external file.
        /// </summary>
        /// <param name="fileName">Name of a existing file in FileFS storage.</param>
        /// <param name="externalPath">Path to new external file to export.</param>
        void Export(string fileName, string externalPath);

        /// <summary>
        /// Returns true if file with given name exists in FileFS storage, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a file to check.</param>
        /// <returns>True if file with given name exists in FileFS storage, otherwise false.</returns>
        bool Exists(string fileName);

        /// <summary>
        /// Returns all files inside FileFS storage.
        /// </summary>
        /// <returns>Enumerable that represents all files inside FileFS storage.</returns>
        IEnumerable<FileEntryInfo> ListFiles();

        /// <summary>
        /// Manually calls optimizer to optimize FileFS storage space.
        /// </summary>
        /// <returns>Count of bytes that was optimized.</returns>
        int ForceOptimize();
    }
}