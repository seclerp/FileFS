namespace FileFs.DataAccess.Exceptions
{
    public class NotEnoughSpaceException : FileFsException
    {
        public NotEnoughSpaceException(string message)
            : base(message)
        {
        }
    }
}