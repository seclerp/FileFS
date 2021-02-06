using FileFs.DataAccess.Exceptions;

namespace FileFS.Managers.Exceptions
{
    public class FileNotFoundException : FileFsException
    {
        public FileNotFoundException(string fileName)
            : base($"File with name '{fileName}' not found in FileFS storage.")
        {
        }
    }
}