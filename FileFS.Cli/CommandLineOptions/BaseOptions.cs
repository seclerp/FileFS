using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    public class BaseOptions
    {
        [Option('i', "instance", Default = "filefs.storage", Required = false, HelpText = "Set filename for FileFS file (instance) to work with.")]
        public string Instance { get; set; }
    }
}