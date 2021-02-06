using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "exists" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("exists", HelpText = "Check that file is exists in FileFS storage.")]
    public class ExistsOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a file to check.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a file to check.")]
        public string FileName { get; set; }
    }
}