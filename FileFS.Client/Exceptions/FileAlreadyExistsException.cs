using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    public class FileAlreadyExistsException : FileFsException
    {
        public FileAlreadyExistsException(string fileName)
            : base($"File with name '{fileName}' already exists in FileFS storage.")
        {
        }
    }
}