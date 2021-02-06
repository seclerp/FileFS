using System;

namespace FileFS.DataAccess.Exceptions
{
    public abstract class FileFsException : Exception
    {
        protected FileFsException(string message)
            : base(message)
        {
        }
    }
}