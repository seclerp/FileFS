using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Base options for every command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class BaseOptions
    {
        /// <summary>
        /// Gets or sets option for specifying FileFS storage file to use.
        /// </summary>
        [Option('i', "instance", Default = "filefs.storage", Required = false, HelpText = "Set filename for FileFS file (instance) to work with.")]
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed logging mode should be enabled.
        /// </summary>
        [Option("debug", Default = false, Required = false, HelpText = "Enable detailed logging during execution of the command.")]
        public bool IsDebug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transaction mode should be enabled.
        /// </summary>
        [Option('t', "transacted", Default = false, Required = false, HelpText = "Enable transaction mode that blocks other CLI instances to work with same storage when current is performing actions.")]
        public bool EnableTransactions { get; set; }

        /// <summary>
        /// Gets or sets a buffer size for streamed read and write.
        /// </summary>
        [Option('b', "buffer-size", Default = 4096, Required = false, HelpText = "Sets a value that will be used as buffer size (in bytes) for streamed read-write operations, such as import and export.")]
        public int ByteBufferSize { get; set; }
    }
}