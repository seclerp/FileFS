namespace FileFS.DataAccess.Exceptions
{
    public class NotEnoughSpaceException : FileFsException
    {
        public NotEnoughSpaceException(string message)
            : base(message)
        {
        }
    }
}