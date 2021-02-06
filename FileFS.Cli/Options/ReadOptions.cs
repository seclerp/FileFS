using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "read" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("read", HelpText = "Read contents of file inside FileFS storage.")]
    public class ReadOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a file to read.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a file to read.")]
        public string FileName { get; set; }
    }
}