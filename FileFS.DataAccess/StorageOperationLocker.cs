using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FileFS.DataAccess.Abstractions;

namespace FileFS.DataAccess
{
    /// <summary>
    /// Implementation of service for locking particular entries or for performing "stop-the-world" locking.
    /// </summary>
    public class StorageOperationLocker : IStorageOperationLocker
    {
        private readonly object _globalLocker;

        private readonly ConcurrentDictionary<int, int> _operationsCountByThreadId;
        private readonly ConcurrentDictionary<string, object> _executingActionLocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageOperationLocker"/> class.
        /// </summary>
        public StorageOperationLocker()
        {
            _operationsCountByThreadId = new ConcurrentDictionary<int, int>();
            _executingActionLocks = new ConcurrentDictionary<string, object>();
            _globalLocker = new object();
        }

        /// <inheritdoc />
        public void LockEntry(string entryId, Action operation)
        {
            LockEntry<object>(entryId, () =>
            {
                operation();
                return default;
            });
        }

        /// <inheritdoc />
        public T LockEntry<T>(string entryId, Func<T> operation)
        {
            if (operation is null)
            {
                return default;
            }

            entryId ??= Guid.NewGuid().ToString();

            // We just need to wait until global lock will be available and don't need to catch it, so Exit immediately
            lock (_globalLocker)
            {
            }

            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            var currentEntryLocker = _executingActionLocks.GetOrAdd(entryId, new object());

            lock (currentEntryLocker)
            {
                _operationsCountByThreadId.AddOrUpdate(currentThreadId, _ => 1, (_, value) => value + 1);

                try
                {
                    return operation();
                }
                finally
                {
                    _operationsCountByThreadId.AddOrUpdate(currentThreadId, _ => 0, (_, value) => value - 1);

                    if (_operationsCountByThreadId[currentThreadId] == 0)
                    {
                        _operationsCountByThreadId.Remove(currentThreadId, out _);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void GlobalLock(Action operation)
        {
            GlobalLock<object>(() =>
            {
                operation();
                return default;
            });
        }

        /// <inheritdoc />
        public T GlobalLock<T>(Func<T> operation)
        {
            if (operation is null)
            {
                return default;
            }

            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            while (_operationsCountByThreadId.Count > 0 && _operationsCountByThreadId.Any(kv => kv.Key != currentThreadId && kv.Value > 0))
            {
                // Wait until all pending operations (except current thread operations) would be processed
            }

            lock (_globalLocker)
            {
                return operation();
            }
        }
    }
}