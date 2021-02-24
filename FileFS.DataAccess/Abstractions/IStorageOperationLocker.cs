using System;

namespace FileFS.DataAccess.Abstractions
{
    public interface IStorageOperationLocker
    {
        void MakeOperation(string entryId, Guid operationId, Action operation);

        void MakeGlobalOperation(Guid currentOperationId, Action operation);

        void MakeGlobalOperation(Action operation);
    }
}