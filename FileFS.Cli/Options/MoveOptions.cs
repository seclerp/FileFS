using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "move" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("move", HelpText = "Move directory or file inside FileFS storage.")]
    public class MoveOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets name of existing directory or file to move.
        /// </summary>
        [Value(0, Required = true, HelpText = "Current name of existing directory or file to move.")]
        public string CurrentName { get; set; }

        /// <summary>
        /// Gets or sets new name of existing directory or file to move.
        /// </summary>
        [Value(1, Required = true, HelpText = "New name of existing directory or file to move.")]
        public string NewName { get; set; }
    }
}