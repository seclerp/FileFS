using System;

namespace FileFs.DataAccess.Exceptions
{
    public class NotEnoughSpaceException : Exception
    {
        public NotEnoughSpaceException(string message)
            : base(message)
        {
        }
    }
}