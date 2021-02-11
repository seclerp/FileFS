using System.IO;
using System.Threading;
using FileFS.DataAccess.Abstractions;
using FileFS.DataAccess.Exceptions;
using Serilog;

namespace FileFS.DataAccess
{
    /// <summary>
    /// File-based storage stream provider.
    /// </summary>
    public class StorageStreamProvider : IStorageStreamProvider
    {
        private readonly ILogger _logger;
        private readonly string _fileFsStoragePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageStreamProvider"/> class.
        /// </summary>
        /// <param name="fileFsStoragePath">Path to FileFS storage file.</param>
        /// <param name="logger">Logger instance.</param>
        public StorageStreamProvider(string fileFsStoragePath, ILogger logger)
        {
            _fileFsStoragePath = fileFsStoragePath;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <exception cref="StorageNotFoundException">Throws when checkExistence is true and storage not exists.</exception>
        public Stream OpenStream(bool checkExistence = true)
        {
            _logger.Information($"Trying to open stream for filename {_fileFsStoragePath}");

            if (checkExistence && !File.Exists(_fileFsStoragePath))
            {
                throw new StorageNotFoundException($"Storage located at file '{_fileFsStoragePath}' not found.");
            }

            var stream = WaitForFileStream(_fileFsStoragePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            _logger.Information($"Stream for filename {_fileFsStoragePath} opened");

            return stream;
        }

        private Stream WaitForFileStream(string fileName, FileMode mode, FileAccess access, FileShare share, int numberOfAttempts = 10)
        {
            int attemptIndex;
            for (attemptIndex = 0; attemptIndex < numberOfAttempts; attemptIndex++)
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = File.Open(_fileFsStoragePath, mode, access, share);
                    return fileStream;
                }
                catch (IOException)
                {
                    fileStream?.Dispose();
                    Thread.Sleep(50);
                }
            }

            throw new StorageNotAvailableException(fileName, attemptIndex + 1);
        }
    }
}