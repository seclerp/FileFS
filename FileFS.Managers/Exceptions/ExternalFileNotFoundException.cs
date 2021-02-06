using FileFs.DataAccess.Exceptions;

namespace FileFS.Managers.Exceptions
{
    public class ExternalFileNotFoundException : FileFsException
    {
        public ExternalFileNotFoundException(string path)
            : base($"File with path '{path}' not found in your filesystem.")
        {
        }
    }
}