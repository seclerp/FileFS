namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when there is not enough space to allocate file data.
    /// </summary>
    public class NotEnoughSpaceException : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughSpaceException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public NotEnoughSpaceException(string message)
            : base(message)
        {
        }
    }
}