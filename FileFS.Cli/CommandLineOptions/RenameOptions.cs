using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("rename", HelpText = "Rename file in FileFS storage.")]
    public class RenameOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Old Name of existing file to rename.")]
        public string OldFileName { get; set; }

        [Value(0, Required = true, HelpText = "New Name of existing file to rename.")]
        public string NewFileName { get; set; }
    }
}