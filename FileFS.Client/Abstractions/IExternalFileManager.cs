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
    }
}