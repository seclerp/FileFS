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
            if (parts.Length < 2)
            {
                return PathConstants.RootDirectoryName;
            }

            return parts.Last();
        }

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