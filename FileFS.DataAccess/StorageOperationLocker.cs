using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Exceptions;

namespace FileFS.DataAccess
{
    public class StorageOperationLocker : IStorageOperationLocker
    {
        private ConcurrentDictionary<Guid, Action> _executingOperations;
        private ConcurrentDictionary<string, object> _executingActionLocks;

        private readonly object _globalLocker;

        public StorageOperationLocker()
        {
            _executingOperations = new ConcurrentDictionary<Guid, Action>();
            _globalLocker = new object();
        }

        public void MakeOperation(string entryId, Guid operationId, Action operation)
        {
            if (operation is null)
            {
                return;
            }

            // We just need to wait until global lock will be available and don't need to catch it, so Exit immediately
            Monitor.Wait(_globalLocker);
            Monitor.Exit(_globalLocker);

            // Concurrent collection is thread-safe so we don't need to synchronize it
            if (!_executingOperations.TryAdd(operationId, operation))
            {
                // TODO: Rewrite to be more descriptive
                throw new OperationIsInvalid(nameof(ConcurrentDictionary<Guid, Action>.TryAdd));
            }

            if (!_executingActionLocks.ContainsKey(entryId))
            {
                if (!_executingActionLocks.TryAdd(entryId, new object()))
                {
                    // TODO: Rewrite to be more descriptive
                    throw new OperationIsInvalid(nameof(ConcurrentDictionary<Guid, object>.TryAdd));
                }
            }

            lock (_executingActionLocks[entryId])
            {
                operation();
                _executingOperations.Remove(operationId, out _);
            }
        }

        public void MakeGlobalOperation(Guid currentOperationId, Action operation)
        {
            if (operation is null)
            {
                return;
            }

            while ( _executingOperations.Keys.Any(key => key != currentOperationId))
            {
                // Wait until all pending operations (except caller operation) would be processed
            }

            lock (_globalLocker)
            {
                operation?.Invoke();
            }
        }

        public void MakeGlobalOperation(Action operation)
        {
            if (operation is null)
            {
                return;
            }

            while (!_executingOperations.IsEmpty)
            {
                // Wait until all pending operations would be processed
            }

            lock (_globalLocker)
            {
                operation?.Invoke();
            }
        }
    }
}