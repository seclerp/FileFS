namespace FileFS.DataAccess.Exceptions
{
    /// <summary>
    /// Exception that should be thrown when user tries to execute invalid operation.
    /// </summary>
    public class OperationIsInvalid : FileFsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationIsInvalid"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public OperationIsInvalid(string message)
            : base(message)
        {
        }
    }
}