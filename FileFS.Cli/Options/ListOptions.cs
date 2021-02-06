using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "list" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("list", HelpText = "List files inside FileFS storage.")]
    public class ListOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether additional details, such as size, created on and updated on will be displayed.
        /// </summary>
        [Option('d', "details", Default = false, Required = false, HelpText = "If true, additional details, such as size, created on and updated on will be displayed.")]
        public bool IsDetailedView { get; set; }
    }
}