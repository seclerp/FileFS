using FileFs.DataAccess.Exceptions;

namespace FileFS.Managers.Exceptions
{
    public class FileAlreadyExistsException : FileFsException
    {
        public FileAlreadyExistsException(string fileName)
            : base($"File with name '{fileName}' already exists in FileFS storage.")
        {
        }
    }
}