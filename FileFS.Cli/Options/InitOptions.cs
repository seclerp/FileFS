using CommandLine;

namespace FileFS.Cli.Options
{
    [Verb("init", HelpText = "Initialize new storage instance of FileFS.")]
    public class InitOptions : BaseOptions
    {
        [Option('s', "size", Default = 10 * 1024 * 1024, Required = false, HelpText = "Size of a newly created storage in bytes.")]
        public int Size { get; set; }

        [Option('p', "pathlength", Default = 256, Required = false, HelpText = "Maximum length of path of the file in bytes.")]
        public int PathLength { get; set; }
    }
}