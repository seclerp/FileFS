using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("delete", HelpText = "Deletes file from FileFS storage.")]
    public class DeleteOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to delete.")]
        public string FileName { get; set; }

        [Option('d', "--force-defrag", Default = false, Required = false, HelpText = "True if storage defragmentation should be executed after deletion, otherwise false")]
        public bool ForceDefrag { get; set; }
    }
}