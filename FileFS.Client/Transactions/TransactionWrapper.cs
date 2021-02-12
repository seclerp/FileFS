using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using FileFS.Client.Constants;
using FileFS.Client.Transactions.Abstractions;
using Serilog;

namespace FileFS.Client.Transactions
{
    /// <summary>
    /// Service which guaranties that all operations between <see cref="BeginTransaction"/> and <see cref="EndTransaction"/> would have exclusive
    /// access to FileFS storage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TransactionWrapper : ITransactionWrapper
    {
        private readonly ILogger _logger;
        private readonly Mutex _mutex;
        private readonly string _mutexName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionWrapper"/> class.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage.</param>
        /// <param name="logger">Logger instance.</param>
        public TransactionWrapper(string fileFsStoragePath, ILogger logger)
        {
            _logger = logger;

            _mutexName = GetMutexName(fileFsStoragePath);
            _mutex = new Mutex(false, _mutexName);
        }

        /// <inheritdoc />
        public void BeginTransaction()
        {
            _logger.Information($"Waiting for mutex {_mutexName}");
            _mutex.WaitOne();
            _logger.Information($"Mutex {_mutexName} acquired");
        }

        /// <inheritdoc />
        public void EndTransaction()
        {
            _mutex.ReleaseMutex();
            _logger.Information($"Mutex {_mutexName} released");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _mutex?.Dispose();
        }

        private static string GetMutexName(string fileFsStoragePath)
        {
            var fullPath = new FileInfo(fileFsStoragePath).FullName;
            var fullPathBytes = Encoding.UTF8.GetBytes(fullPath);
            var fullPathBase64 = Convert.ToBase64String(fullPathBytes);
            var mutexName = string.Format(SynchronizationConstants.MutexNameTemplate, fullPathBase64);

            return mutexName;
        }
    }
}