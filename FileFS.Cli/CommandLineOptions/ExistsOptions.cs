using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("exists", HelpText = "Check that file is exists in FileFS storage.")]
    public class ExistsOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to check.")]
        public string FileName { get; set; }
    }
}