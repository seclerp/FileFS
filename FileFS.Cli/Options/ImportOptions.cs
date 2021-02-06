using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "import" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("import", HelpText = "Imports existing file in your filesystem to a new file inside FileFS storage.")]
    public class ImportOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets path to an existing external file to import.
        /// </summary>
        [Value(0, Required = true, HelpText = "Path to an existing external file to import.")]
        public string ImportPath { get; set; }

        /// <summary>
        /// Gets or sets name of a new file inside FileFS storage.
        /// </summary>
        [Value(1, Required = true, HelpText = "Name of a new file inside FileFS storage.")]
        public string FileName { get; set; }
    }
}