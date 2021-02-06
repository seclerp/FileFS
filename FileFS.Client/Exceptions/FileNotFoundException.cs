using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    public class FileNotFoundException : FileFsException
    {
        public FileNotFoundException(string fileName)
            : base($"File with name '{fileName}' not found in FileFS storage.")
        {
        }
    }
}