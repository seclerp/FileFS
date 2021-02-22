using System;
using FileFS.DataAccess.Entities.Abstractions;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Directory entry implementation.
    /// </summary>
    public readonly struct DirectoryEntry : IEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryEntry"/> struct.
        /// </summary>
        /// <param name="id">Id of the directory.</param>
        /// <param name="entryName">Name of the directory.</param>
        /// <param name="parentEntryId">Id of the parent directory.</param>
        public DirectoryEntry(Guid id, string entryName, Guid parentEntryId)
        {
            Id = id;
            EntryName = entryName;
            ParentEntryId = parentEntryId;
        }

        /// <summary>
        /// Gets id of the directory.
        /// </summary>
        public Guid Id { get; }

        /// <inheritdoc />
        public string EntryName { get; }

        /// <inheritdoc />
        public Guid ParentEntryId { get; }
    }
}