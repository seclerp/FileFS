using System;

namespace FileFS.Client.Transactions.Abstractions
{
    /// <summary>
    /// Abstraction for transaction wrapper, service, which guaranties that all operations between <see cref="BeginTransaction"/> and <see cref="EndTransaction"/> would have exclusive
    /// access to FileFS storage.
    /// </summary>
    public interface ITransactionWrapper : IDisposable
    {
        /// <summary>
        /// Starts new transaction to FileFS storage.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Ends transaction to FileFS storage.
        /// </summary>
        void EndTransaction();
    }
}