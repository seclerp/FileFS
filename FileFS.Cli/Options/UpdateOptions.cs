using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "update" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("update", HelpText = "Update file content inside FileFS storage.")]
    public class UpdateOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of a file to update.
        /// </summary>
        [Value(0, Required = true, HelpText = "Name of a existing file to update.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets new content of a existing file.
        /// </summary>
        [Value(1, Default = "", Required = false, HelpText = "New content of a existing file.")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storage should be optimized after update.
        /// </summary>
        [Option('o', "force-optimize", Default = false, Required = false, HelpText = "True if storage should be optimized directly after update, otherwise false")]
        public bool ForceOptimize { get; set; }
    }
}