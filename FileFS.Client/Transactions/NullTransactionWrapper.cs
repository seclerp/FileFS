using FileFS.Client.Transactions.Abstractions;

namespace FileFS.Client.Transactions
{
    /// <summary>
    /// Null-object implementation of <see cref="ITransactionWrapper"/>.
    /// </summary>
    public class NullTransactionWrapper : ITransactionWrapper
    {
        /// <inheritdoc />
        public void BeginTransaction()
        {
        }

        /// <inheritdoc />
        public void EndTransaction()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}