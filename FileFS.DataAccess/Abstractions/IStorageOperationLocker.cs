using System;

namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction that represents service for locking particular entries or for performing "stop-the-world" locking.
    /// </summary>
    public interface IStorageOperationLocker
    {
        /// <summary>
        /// Locks specific entries to prevent access from another threads and executes given operation.
        /// </summary>
        /// <param name="entryId">Unique entry identifier used for lock checking.</param>
        /// <param name="operation">Operation that need to be executed under locking.</param>
        void LockEntry(string entryId, Action operation);

        /// <summary>
        /// Locks specific entries to prevent access from another threads and executes given operation.
        /// </summary>
        /// <param name="entryId">Unique entry identifier used for lock checking.</param>
        /// <param name="operation">Operation that need to be executed under locking.</param>
        /// <typeparam name="T">Type of resulting value.</typeparam>
        /// <returns>Result of operation execution.</returns>
        T LockEntry<T>(string entryId, Func<T> operation);

        /// <summary>
        /// Waits for all operations from others thread to complete, locks entire storage to perform global operation.
        /// </summary>
        /// <param name="operation">Global operation to perform under locking.</param>
        void GlobalLock(Action operation);

        /// <summary>
        /// Waits for all operations from others thread to complete, locks entire storage to perform global operation.
        /// </summary>
        /// <param name="operation">Global operation to perform under locking.</param>
        /// <typeparam name="T">Type of resulting value.</typeparam>
        /// <returns>Result of operation execution.</returns>
        T GlobalLock<T>(Func<T> operation);
    }
}