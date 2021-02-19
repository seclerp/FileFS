using System;
using FileFS.DataAccess.Entities.Enums;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents meta information about the entry inside FileFS storage.
    /// </summary>
    public readonly struct FileFsEntryInfo
    {
        /// <summary>
        /// Name of the entry.
        /// </summary>
        public readonly string EntryName;

        /// <summary>
        /// Type of the entry.
        /// </summary>
        public readonly EntryType EntryType;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public readonly int Size;

        /// <summary>
        /// DateTime instance that represents time when file was created.
        /// </summary>
        public readonly DateTime CreatedOn;

        /// <summary>
        /// DateTime instance that represents time when file was updated last time.
        /// </summary>
        public readonly DateTime UpdatedOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFsEntryInfo"/> struct.
        /// </summary>
        /// <param name="entryName">Name of the file.</param>
        /// <param name="entryType">Type of the entry.</param>
        /// <param name="size">File size in bytes.</param>
        /// <param name="createdOn">DateTime instance that represents time when file was created.</param>
        /// <param name="updatedOn">DateTime instance that represents time when file was updated last time.</param>
        public FileFsEntryInfo(string entryName, EntryType entryType, int size, DateTime createdOn, DateTime updatedOn)
        {
            EntryName = entryName;
            EntryType = entryType;
            Size = size;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }
    }
}