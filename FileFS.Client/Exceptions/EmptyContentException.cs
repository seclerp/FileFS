using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    public class EmptyContentException : FileFsException
    {
        public EmptyContentException(string fileName)
            : base($"Content for file '{fileName}' must be non-emoty.")
        {
        }
    }
}