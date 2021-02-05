using System.IO;
using Microsoft.Extensions.Logging;

namespace FileFS.Managers
{
    public class ExternalFileManager : IExternalFileManager
    {
        private readonly ILogger<ExternalFileManager> _logger;

        public ExternalFileManager(ILogger<ExternalFileManager> logger)
        {
            _logger = logger;
        }

        public void Write(string externalFileName, byte[] data)
        {
            _logger.LogInformation($"Start external file write, filename {externalFileName}, data length {data.Length}");

            File.WriteAllBytes(externalFileName, data);

            _logger.LogInformation($"Done external file write, filename {externalFileName}, data length {data.Length}");
        }

        public byte[] Read(string externalFileName)
        {
            _logger.LogInformation($"Start external file read, filename {externalFileName}");

            var bytes = File.ReadAllBytes(externalFileName);

            _logger.LogInformation($"Done external file read, filename {externalFileName}, bytes count {bytes.Length}");

            return bytes;
        }
    }
}