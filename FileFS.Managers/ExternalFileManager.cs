using System.IO;
using Serilog;

namespace FileFS.Managers
{
    public class ExternalFileManager : IExternalFileManager
    {
        private readonly ILogger _logger;

        public ExternalFileManager(ILogger logger)
        {
            _logger = logger;
        }

        public void Write(string externalFileName, byte[] data)
        {
            _logger.Information($"Start external file write, filename {externalFileName}, data length {data.Length}");

            File.WriteAllBytes(externalFileName, data);

            _logger.Information($"Done external file write, filename {externalFileName}, data length {data.Length}");
        }

        public byte[] Read(string externalFileName)
        {
            _logger.Information($"Start external file read, filename {externalFileName}");

            var bytes = File.ReadAllBytes(externalFileName);

            _logger.Information($"Done external file read, filename {externalFileName}, bytes count {bytes.Length}");

            return bytes;
        }
    }
}