using System.IO;
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

            var stream = File.OpenWrite(_fileFsStoragePath);

            _logger.Information($"Stream for filename {_fileFsStoragePath} opened");

            return stream;
        }
    }
}