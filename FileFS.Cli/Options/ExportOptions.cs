using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "export" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("export", HelpText = "Exports file from FileFS storage to a new file in your filesystem.")]
    public class ExportOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a existing file inside FileFS storage.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a existing file inside FileFS storage.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets path to a new external file for export.
        /// </summary>
        [Value(1, Required = true, HelpText = "Path to a new external file for export.")]
        public string ExportPath { get; set; }
    }
}