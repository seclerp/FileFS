using System;
using FileFS.DataAccess.Entities.Enums;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents entry descriptor in FileFS storage.
    /// </summary>
    public readonly struct EntryDescriptor
    {
        /// <summary>
        /// Amount of bytes used to store file descriptor data, except filename.
        /// </summary>
        public static readonly int BytesWithoutFilename = 61;

        /// <summary>
        /// Unique ID of entry.
        /// </summary>
        public readonly Guid Id; // 16

        /// <summary>
        /// ID of the parent entry.
        /// </summary>
        public readonly Guid ParentId; // 16

        /// <summary>
        /// Actual length of the name.
        /// </summary>
        public readonly int NameLength; // 4

        /// <summary>
        /// Name of the entry.
        /// </summary>
        public readonly string Name; // N

        /// <summary>
        /// Type of the entry.
        /// </summary>
        public readonly EntryType Type; // 1

        /// <summary>
        /// Unix timestamp that represents time when entry was created.
        /// </summary>
        public readonly long CreatedOn; // 8

        /// <summary>
        /// Unix timestamp that represents time when entry was changed last time.
        /// </summary>
        public readonly long UpdatedOn; // 8

        /// <summary>
        /// Offset of the file data in memory.
        /// </summary>
        public readonly int DataOffset; // 4

        /// <summary>
        /// Length of the file data.
        /// </summary>
        public readonly int DataLength; // 4

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryDescriptor"/> struct.
        /// </summary>
        /// <param name="id">Unique ID of a entry descriptor.</param>
        /// <param name="parentId">ID of the parent entry.</param>
        /// <param name="name">Name of the entry.</param>
        /// <param name="type">Type of the entry stored in descriptor.</param>
        /// <param name="createdOn">Unix timestamp that represents time when entry was created.</param>
        /// <param name="updatedOn">Unix timestamp that represents time when entry was changed last time.</param>
        /// <param name="dataOffset">Offset of the entry data in memory.</param>
        /// <param name="dataLength">Length of the entry data.</param>
        public EntryDescriptor(Guid id, Guid parentId, string name, EntryType type, long createdOn, long updatedOn, int dataOffset, int dataLength)
        {
            Id = id;
            ParentId = parentId;
            NameLength = name.Length;
            Name = name;
            Type = type;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }
    }
}