using CommandLine;

namespace FileFS.Cli.Options
{
    [Verb("create", HelpText = "Create new file inside FileFS storage.")]
    public class CreateOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Name of a file to create.")]
        public string FileName { get; set; }

        [Value(1, Required = true, HelpText = "Content of a newly created file.")]
        public string Content { get; set; }
    }
}