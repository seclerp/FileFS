using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("export", HelpText = "Exports file from FileFS storage to a new file in your filesystem.")]
    public class ExportOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a existing file inside FileFS storage.")]
        public string FileName { get; set; }

        [Value(1, Required = true, HelpText = "Path to a new file for export.")]
        public string ExportPath { get; set; }
    }
}