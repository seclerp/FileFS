namespace FileFS.Cli.Extensions
{
    /// <summary>
    /// Extensions for filename represented as string.
    /// </summary>
    internal static class FileNameExtensions
    {
        /// <summary>
        /// Returns clipped fixed size filename.
        /// </summary>
        /// <param name="fileName">Filename value.</param>
        /// <param name="length">Fixed length to clip to.</param>
        /// <returns>Original filename if length of filename string less or equals "length" and clipped fixed size filename if not.</returns>
        internal static string Clip(this string fileName, int length)
        {
            if (fileName.Length <= length)
            {
                return fileName;
            }

            return $"{fileName.Substring(0, length)}...";
        }
    }
}