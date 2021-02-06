using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "rename" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("rename", HelpText = "Rename file in FileFS storage.")]
    public class RenameOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets old name of existing file to rename.
        /// </summary>
        [Value(0, Required = true, HelpText = "Old name of existing file to rename.")]
        public string OldFileName { get; set; }

        /// <summary>
        /// Gets or sets new name of existing file to rename.
        /// </summary>
        [Value(0, Required = true, HelpText = "New name of existing file to rename.")]
        public string NewFileName { get; set; }
    }
}