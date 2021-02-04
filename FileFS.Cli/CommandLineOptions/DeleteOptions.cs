using CommandLine;

namespace FileFS.Cli.CommandLineOptions
{
    [Verb("delete", HelpText = "Deletes file from FileFS storage.")]
    public class DeleteOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to delete.")]
        public string FileName { get; set; }

        [Option('o', "force-optimize", Default = false, Required = false, HelpText = "True if storage should be optimized directly after deletion, otherwise false")]
        public bool ForceOptimize { get; set; }
    }
}