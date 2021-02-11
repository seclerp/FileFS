using System;
using System.Collections.Generic;
using FileFS.DataAccess.Entities;

namespace FileFS.DataAccess.Tests.Comparers
{
    /// <summary>
    /// Comparer implementation for <see cref="FileEntryInfo"/>.
    /// </summary>
    public class FileEntryInfoEqualityComparer : IEqualityComparer<FileEntryInfo>
    {
        /// <inheritdoc />
        public bool Equals(FileEntryInfo x, FileEntryInfo y)
        {
            // We dont check for created and updated time here because it is tricky to mock it in the right way
            return x.FileName == y.FileName && x.Size == y.Size;
        }

        /// <inheritdoc />
        public int GetHashCode(FileEntryInfo obj)
        {
            return HashCode.Combine(obj.FileName, obj.Size, obj.CreatedOn, obj.UpdatedOn);
        }
    }
}