using CommandLine;

namespace FileFS.Cli.Options
{
    [Verb("update", HelpText = "Update file content inside FileFS storage.")]
    public class UpdateOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to update.")]
        public string FileName { get; set; }

        [Value(1, Required = true, HelpText = "New content of a file.")]
        public string Content { get; set; }

        [Option('o', "force-optimize", Default = false, Required = false, HelpText = "True if storage should be optimized directly after update, otherwise false")]
        public bool ForceOptimize { get; set; }
    }
}