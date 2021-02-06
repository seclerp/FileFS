using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "delete" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("delete", HelpText = "Deletes file from FileFS storage.")]
    public class DeleteOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a file to delete.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a file to delete.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storage should be optimized after deletion.
        /// </summary>
        [Option('o', "force-optimize", Default = false, Required = false, HelpText = "True if storage should be optimized after deletion, otherwise false")]
        public bool ForceOptimize { get; set; }
    }
}