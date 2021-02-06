using CommandLine;

namespace FileFS.Cli.Options
{
    [Verb("import", HelpText = "Imports existing file in your filesystem to a new file inside FileFS storage.")]
    public class ImportOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Path to an existing file to import.")]
        public string ImportPath { get; set; }

        [Value(1, Required = true, HelpText = "Name of a new file inside FileFS storage.")]
        public string FileName { get; set; }
    }
}