using System;

namespace FileFS.DataAccess.Entities.Abstractions
{
    /// <summary>
    /// Abstraction for every file entry implementation.
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Gets name of the entry.
        /// </summary>
        public string EntryName { get; }

        /// <summary>
        /// Path to a directory where entry should be stored.
        /// </summary>
        public Guid ParentEntryId { get; }
    }
}