using System;
using FileFS.DataAccess.Entities.Abstractions;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Implementation for placeholder file entry, that represents empty file of given size.
    /// </summary>
    public struct PlaceholderFileEntry : IFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholderFileEntry"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parentEntryId">ID of the parent entry.</param>
        /// <param name="dataLength">Length of a data for placeholder.</param>
        public PlaceholderFileEntry(string fileName, Guid parentEntryId, int dataLength)
        {
            EntryName = fileName;
            ParentEntryId = parentEntryId;
            DataLength = dataLength;
        }

        /// <inheritdoc />
        public string EntryName { get; }

        /// <inheritdoc />
        public Guid ParentEntryId { get; }

        /// <inheritdoc />
        public int DataLength { get; }
    }
}