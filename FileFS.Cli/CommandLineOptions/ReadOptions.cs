using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("read", HelpText = "Read contents of file inside FileFS storage.")]
    public class ReadOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to read.")]
        public string FileName { get; set; }
    }
}