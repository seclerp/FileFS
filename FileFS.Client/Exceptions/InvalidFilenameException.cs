using FileFS.Client.Constants;
using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Exceptions
{
    public class InvalidFilenameException : FileFsException
    {
        public InvalidFilenameException(string fileName)
            : base($"'{fileName}' is invalid. Filename must match pattern {PatternMatching.ValidFilename}.")
        {
        }
    }
}