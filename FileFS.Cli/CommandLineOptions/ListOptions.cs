using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("list", HelpText = "List files inside FileFS storage.")]
    public class ListOptions : BaseOptions
    {
    }
}