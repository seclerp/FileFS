using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "create" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("create", HelpText = "Create new file inside FileFS storage.")]
    public class CreateOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a file to create.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a file to create.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets content of a newly created file.
        /// </summary>
        [Value(1, Required = true, HelpText = "Content of a newly created file.")]
        public string Content { get; set; }
    }
}