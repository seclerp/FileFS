using System.IO;

namespace FileFS.Client.Abstractions
{
    /// <summary>
    /// Abstraction for external (non FileFS managed) files manipulation.
    /// </summary>
    public interface IExternalFileManager
    {
        /// <summary>
        /// Writes given data into external file.
        /// </summary>
        /// <param name="externalFileName">Path to the external file.</param>
        /// <param name="data">Data bytes to write.</param>
        void Write(string externalFileName, byte[] data);

        /// <summary>
        /// Reads data bytes from external file.
        /// </summary>
        /// <param name="externalFileName">Path to the external file.</param>
        /// <returns>Data bytes read from external file.</returns>
        byte[] Read(string externalFileName);

        /// <summary>
        /// Opens stream for reading from external file.
        /// </summary>
        /// <param name="externalFileName">Path to the external file.</param>
        /// <returns>Ready for reading stream with external file data.</returns>
        Stream OpenReadStream(string externalFileName);

        /// <summary>
        /// Opens stream for writing to external file.
        /// </summary>
        /// <param name="externalFileName">Path to the external file.</param>
        /// <returns>Ready for writing stream with external file data.</returns>
        Stream OpenWriteStream(string externalFileName);

        /// <summary>
        /// Returns true if external file exists, otherwise false.
        /// </summary>
        /// <param name="externalFileName">Path to the external file.</param>
        /// <returns>True if external file exists, otherwise false.</returns>
        bool Exists(string externalFileName);
    }
}