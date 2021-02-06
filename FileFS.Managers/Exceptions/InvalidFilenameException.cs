using FileFs.DataAccess.Exceptions;
using FileFS.Managers.Constants;

namespace FileFS.Managers.Exceptions
{
    public class InvalidFilenameException : FileFsException
    {
        public InvalidFilenameException(string fileName)
            : base($"'{fileName}' is invalid. Filename must match pattern {PatternMatching.ValidFilename}.")
        {
        }
    }
}