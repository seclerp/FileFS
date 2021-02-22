using System;
using System.Collections.Generic;
using FileFS.DataAccess.Entities;

namespace FileFS.Tests.Shared.Comparers
{
    /// <summary>
    /// Comparer implementation for <see cref="FileFsEntryInfo"/>.
    /// </summary>
    public class FileEntryInfoEqualityComparer : IEqualityComparer<FileFsEntryInfo>
    {
        /// <inheritdoc />
        public bool Equals(FileFsEntryInfo x, FileFsEntryInfo y)
        {
            // We dont check for created and updated time here because it is tricky to mock it in the right way
            return x.EntryName == y.EntryName && x.Size == y.Size;
        }

        /// <inheritdoc />
        public int GetHashCode(FileFsEntryInfo obj)
        {
            return HashCode.Combine(obj.EntryName, obj.Size, obj.CreatedOn, obj.UpdatedOn);
        }
    }
}