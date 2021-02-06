using CommandLine;

namespace FileFS.Cli.Options
{
    [Verb("list", HelpText = "List files inside FileFS storage.")]
    public class ListOptions : BaseOptions
    {
        [Option('d', "details", Default = false, Required = false, HelpText = "If true, additional details, such as size, created on and updated on will be displayed.")]
        public bool IsDetailedView { get; set; }
    }
}