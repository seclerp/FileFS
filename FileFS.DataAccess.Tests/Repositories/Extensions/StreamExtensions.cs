using System;
using System.IO;

namespace FileFS.DataAccess.Tests.Repositories.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Stream"/> type.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Returns true if content of both streams is the same.
        /// </summary>
        /// <param name="self">First stream to check.</param>
        /// <param name="other">Second stream to check.</param>
        /// <returns>True if content of both streams is the same, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Throws when one of the streams is null.</exception>
        public static bool StreamEquals(this Stream self, Stream other)
        {
            if (self == other)
            {
                return true;
            }

            if (self == null || other == null)
            {
                throw new ArgumentNullException(self == null ? "self" : "other");
            }

            if (self.Length != other.Length)
            {
                return false;
            }

            self.Seek(0, SeekOrigin.Begin);
            other.Seek(0, SeekOrigin.Begin);
            for (var i = 0; i < self.Length; i++)
            {
                var aByte = self.ReadByte();
                var bByte = other.ReadByte();
                if (aByte.CompareTo(bByte) != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}