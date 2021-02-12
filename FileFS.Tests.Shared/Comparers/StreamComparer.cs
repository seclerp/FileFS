using System;
using System.Collections.Generic;
using System.IO;

namespace FileFS.Tests.Shared.Comparers
{
    /// <summary>
    /// Comparer implementation for <see cref="Stream"/>.
    /// </summary>
    public class StreamComparer : IEqualityComparer<Stream>
    {
        /// <inheritdoc />
        public bool Equals(Stream x, Stream y)
        {
            if (x == y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                throw new ArgumentNullException(x == null ? "self" : "other");
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            x.Seek(0, SeekOrigin.Begin);
            y.Seek(0, SeekOrigin.Begin);
            for (var i = 0; i < x.Length; i++)
            {
                var aByte = x.ReadByte();
                var bByte = y.ReadByte();
                if (aByte.CompareTo(bByte) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public int GetHashCode(Stream obj)
        {
            return obj.GetHashCode();
        }
    }
}