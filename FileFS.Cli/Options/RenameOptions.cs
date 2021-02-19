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
        /// Gets or sets name of existing directory or file to rename.
        /// </summary>
        [Value(0, Required = true, HelpText = "Current name of existing directory or file to to rename.")]
        public string CurrentName { get; set; }

        /// <summary>
        /// Gets or sets new name of existing directory or file to move.
        /// </summary>
        [Value(1, Required = true, HelpText = "New name of existing directory or file to rename.")]
        public string NewName { get; set; }
    }
}