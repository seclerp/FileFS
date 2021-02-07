using CommandLine;

namespace FileFS.Cli.Options
{
    /// <summary>
    /// Options for "init" command.
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [Verb("init", HelpText = "Initialize new storage instance of FileFS.")]
    public class InitOptions : BaseOptions
    {
        /// <summary>
        /// Gets or sets size of a newly created storage in bytes.
        /// </summary>
        [Option('s', "size", Default = 10 * 1024 * 1024, Required = false, HelpText = "Size of a newly created storage in bytes.")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets maximum length of name of the file in bytes.
        /// </summary>
        [Option('n', "name-length", Default = 256, Required = false, HelpText = "Maximum length of name of the file in bytes.")]
        public int FileNameLength { get; set; }
    }
}