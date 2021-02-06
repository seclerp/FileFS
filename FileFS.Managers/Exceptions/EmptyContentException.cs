using FileFs.DataAccess.Exceptions;

namespace FileFS.Managers.Exceptions
{
    public class EmptyContentException : FileFsException
    {
        public EmptyContentException(string fileName)
            : base($"Content for file '{fileName}' must be non-emoty.")
        {
        }
    }
}