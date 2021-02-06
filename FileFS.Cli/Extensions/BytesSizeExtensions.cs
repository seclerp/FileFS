using System;
using System.Globalization;

namespace FileFS.Cli.Extensions
{
    /// <summary>
    /// Extensions for bytes size represented as 64-bit integer.
    /// </summary>
    internal static class BytesSizeExtensions
    {
        private static readonly string[] Measures = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        /// <summary>
        /// Returns optimal string representation of given bytes size represented as 64-bit integer.
        /// </summary>
        /// <param name="bytesSize">Bytes size.</param>
        /// <returns>Optimal string representation of given bytes size.</returns>
        internal static string FormatBytesSize(this long bytesSize)
        {
            if (bytesSize == 0)
            {
                return $"0{Measures[0]}";
            }

            var bytes = Math.Abs(bytesSize);
            var measureIndex = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var roundedSize = Math.Round(bytes / Math.Pow(1024, measureIndex), 1);

            return (Math.Sign(bytesSize) * roundedSize).ToString(CultureInfo.InvariantCulture) + Measures[measureIndex];
        }
    }
}