namespace FileFS.Cli.Extensions
{
    internal static class FileNameExtensions
    {
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