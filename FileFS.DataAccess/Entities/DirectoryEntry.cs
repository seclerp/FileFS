using System;
using FileFS.DataAccess.Entities.Abstractions;

namespace FileFS.DataAccess.Entities
{
    public struct DirectoryEntry : IEntry
    {
        public DirectoryEntry(Guid id, string entryName, Guid parentEntryId)
        {
            Id = id;
            EntryName = entryName;
            ParentEntryId = parentEntryId;
        }

        public Guid Id { get; }

        public string EntryName { get; }

        public Guid ParentEntryId { get; }
    }
}