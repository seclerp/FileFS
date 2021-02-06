using System;

namespace FileFs.DataAccess.Exceptions
{
    public abstract class FileFsException : Exception
    {
        protected FileFsException(string message)
            : base(message)
        {
        }
    }
}