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
    }
}