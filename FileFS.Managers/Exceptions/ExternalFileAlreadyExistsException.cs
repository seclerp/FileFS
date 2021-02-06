using FileFs.DataAccess.Exceptions;

namespace FileFS.Managers.Exceptions
{
    public class ExternalFileAlreadyExistsException : FileFsException
    {
        public ExternalFileAlreadyExistsException(string path)
            : base($"File with path '{path}' already exists in your filesystem.")
        {
        }
    }
}