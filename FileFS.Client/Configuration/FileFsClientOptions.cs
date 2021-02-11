namespace FileFS.Client.Configuration
{
    /// <summary>
    /// Type that represents options for <see cref="FileFsClient"/>.
    /// </summary>
    public class FileFsClientOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether all client operations should be transacted.
        /// This could slow overall operation performance.
        /// </summary>
        public bool EnableTransactions { get; set; } = false;

        /// <summary>
        /// Gets or sets byte buffer size used in streamed Read/Write operations.
        /// </summary>
        public int ByteBufferSize { get; set; } = 4096;
    }
}