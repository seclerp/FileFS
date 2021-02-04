using System.Collections.Generic;
using FileFS.Managers.Models;

namespace FileFS.Api.Abstractions
{
    /// <summary>
    /// Interface that represents entry point to interact with FileFS.
    /// </summary>
    public interface IFileFsClient
    {
        /// <summary>
        /// Create a file with given filename and content.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        /// <param name="content">File contents.</param>
        void Create(string fileName, byte[] content);

        /// <summary>
        /// Updates a file with given filename using new content.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        /// <param name="newContent">New file content.</param>
        void Update(string fileName, byte[] newContent);

        /// <summary>
        /// Deletes file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        void Delete(string fileName);

        /// <summary>
        /// Returns content of a file with given filename.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        /// <returns>Byte array representing file contents.</returns>
        byte[] Read(string fileName);

        /// <summary>
        /// Return true if file exists, otherwise false.
        /// </summary>
        /// <param name="fileName">Name of a file.</param>
        /// <returns>True if file exists, otherwise false.</returns>
        bool Exists(string fileName);

        /// <summary>
        /// Renames file with given "oldName" to a "newName".
        /// </summary>
        /// <param name="oldName">Old name of a file.</param>
        /// <param name="newName">New name of a file.</param>
        void Rename(string oldName, string newName);

        IReadOnlyCollection<EntryInfo> List();

        void ForceOptimize();
    }
}