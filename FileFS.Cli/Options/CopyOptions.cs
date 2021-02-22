using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "copy" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("copy", HelpText = "Copy directory or file inside FileFS storage.")]
    public class CopyOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of existing directory or file to move.
        /// </summary>
        [Value(0, Required = true, HelpText = "Current name of existing directory or file to copy.")]
        public string CurrentName { get; set; }

        /// <summary>
        /// Gets or sets new name of existing directory or file to move.
        /// </summary>
        [Value(1, Required = true, HelpText = "New name of existing directory or file to copy.")]
        public string NewName { get; set; }
    }
}