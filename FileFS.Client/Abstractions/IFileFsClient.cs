using System.Collections.Generic;
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
        /// Creates new directory.
        /// </summary>
        /// <param name="name">Name of a directory to create.</param>
        void CreateDirectory(string name);

        /// <summary>
        /// Creates new file with empty data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        void CreateFile(string fileName);

        /// <summary>
        /// Creates new file with given data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        /// <param name="data">Data bytes.</param>
        void CreateFile(string fileName, byte[] data);

        /// <summary>
        /// Creates new file with given data.
        /// </summary>
        /// <param name="fileName">Name of a file to create.</param>
        /// <param name="sourceStream">Source stream of bytes.</param>
        /// <param name="length">Length of data to be read in bytes.</param>
        void CreateFile(string fileName, Stream sourceStream, int length);

        /// <summary>
        /// Updates existing file with new data.
        /// </summary>
        /// <param name="fileName">Name of a file to update.</param>
        /// <param name="newData">New data bytes.</param>
        void Update(string fileName, byte[] newData);

        /// <summary>
        /// Updates existing file with new data.
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
        /// Change name of existing file or directory.
        /// </summary>
        /// <param name="currentName">Current name of file.</param>
        /// <param name="newName">New name of a file.</param>
        void Rename(string currentName, string newName);

        /// <summary>
        /// Deletes existing directory.
        /// </summary>
        /// <param name="name">Name of a directory to delete.</param>
        void DeleteDirectory(string name);

        /// <summary>
        /// Deletes existing file.
        /// </summary>
        /// <param name="fileName">Name of a file to delete.</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Imports external file into FileFS storage.
        /// </summary>
        /// <param name="externalPath">Path to external file to import.</param>
        /// <param name="fileName">Name of a new file in FileFS storage.</param>
        void ImportFile(string externalPath, string fileName);

        /// <summary>
        /// Exports file from FileFS storage to new external file.
        /// </summary>
        /// <param name="fileName">Name of a existing file in FileFS storage.</param>
        /// <param name="externalPath">Path to new external file to export.</param>
        void ExportFile(string fileName, string externalPath);

        /// <summary>
        /// Returns true if file with given name exists in FileFS storage, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a file to check.</param>
        /// <returns>True if file with given name exists in FileFS storage, otherwise false.</returns>
        bool FileExists(string fileName);

        /// <summary>
        /// Returns true if directory with given name exists in FileFS storage, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a directory to check.</param>
        /// <returns>True if directory with given name exists in FileFS storage, otherwise false.</returns>
        bool DirectoryExists(string fileName);

        /// <summary>
        /// Returns true if file or directory with given name exists in FileFS storage, otherwise false.
        /// </summary>
        /// <param name="name">Name of a file or directory to check.</param>
        /// <returns>True if file or directory with given name exists in FileFS storage, otherwise false.</returns>
        bool Exists(string name);

        /// <summary>
        /// Returns all files and directories inside FileFS storage.
        /// </summary>
        /// <returns>Enumerable that represents all files inside FileFS storage.</returns>
        IEnumerable<FileFsEntryInfo> ListFiles();

        /// <summary>
        /// Returns true if entry with specified name is a directory, otherwise false.
        /// </summary>
        /// <param name="name">Name of an entry.</param>
        /// <returns>True if entry with specified name is a directory, otherwise false.</returns>
        bool IsDirectory(string name);

        /// <summary>
        /// Manually calls optimizer to optimize FileFS storage space.
        /// </summary>
        /// <returns>Count of bytes that was optimized.</returns>
        int ForceOptimize();
    }
}