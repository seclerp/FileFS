using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "create-dir" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("create-dir", HelpText = "Create new file inside FileFS storage.")]
    public class CreateDirectoryOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a directory to create.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a directory to create.")]
        public string DirectoryName { get; set; }
    }
}