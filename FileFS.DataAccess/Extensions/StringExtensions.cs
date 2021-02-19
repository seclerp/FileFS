using System;
using System.Linq;
using FileFS.DataAccess.Constants;

namespace FileFS.DataAccess.Extensions
{
    /// <summary>
    /// Extensions for <see cref="string"/> type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns short name (without full path) of a full name.
        /// </summary>
        /// <param name="entryName">Full name of an entry.</param>
        /// <returns>Short name (without full path) of a full name.</returns>
        public static string GetShortName(this string entryName)
        {
            var parts = entryName.Split(PathConstants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            // Entry in root directory or it is root directory
            if (parts.Length is 0)
            {
                return PathConstants.RootDirectoryName;
            }

            return parts.Last();
        }

        /// <summary>
        /// Returns full name of the parent entry.
        /// </summary>
        /// <param name="entryName">Full name of an entry.</param>
        /// <returns>Full name of the parent entry.</returns>
        public static string GetParentFullName(this string entryName)
        {
            var parts = entryName.Split(PathConstants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            // Entry in root directory or it is root directory
            if (parts.Length < 2)
            {
                return PathConstants.RootDirectoryName;
            }

            return $"{PathConstants.RootDirectoryName}{string.Join(PathConstants.PathSeparator, parts.Take(parts.Length - 1))}";
        }

        /// <summary>
        /// Combines current and other path parts into one.
        /// </summary>
        /// <param name="first">Current path part.</param>
        /// <param name="second">Another path part.</param>
        /// <returns>Combined path.</returns>
        public static string CombineWith(this string first, string second)
        {
            if (first == PathConstants.RootDirectoryName)
            {
                return $"{PathConstants.RootDirectoryName}{second}";
            }

            return $"{first}{PathConstants.PathSeparator}{second}";
        }
    }
}