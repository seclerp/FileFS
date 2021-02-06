using System.IO;
using FileFS.Client.Abstractions;
using Serilog;

namespace FileFS.Client
{
    /// <summary>
    /// Implementation of manager for external (non FileFS managed) files manipulation.
    /// </summary>
    public class ExternalFileManager : IExternalFileManager
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFileManager"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ExternalFileManager(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void Write(string externalFileName, byte[] data)
        {
            _logger.Information($"Start external file write, filename {externalFileName}, data length {data.Length}");

            File.WriteAllBytes(externalFileName, data);

            _logger.Information($"Done external file write, filename {externalFileName}, data length {data.Length}");
        }

        /// <inheritdoc />
        public byte[] Read(string externalFileName)
        {
            _logger.Information($"Start external file read, filename {externalFileName}");

            var bytes = File.ReadAllBytes(externalFileName);

            _logger.Information($"Done external file read, filename {externalFileName}, bytes count {bytes.Length}");

            return bytes;
        }
    }
}