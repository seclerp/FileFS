using CommandLine;
using FileFS.Cli.Options;

namespace FileFS.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                    InitOptions,
                    CreateOptions,
                    UpdateOptions,
                    DeleteOptions,
                    ImportOptions,
                    ExportOptions,
                    RenameOptions,
                    ExistsOptions,
                    ReadOptions,
                    ListOptions
                >(args)
                .WithParsed<InitOptions>(CommandHandlers.HandleInit)
                .WithParsed<CreateOptions>(CommandHandlers.HandleCreate)
                .WithParsed<UpdateOptions>(CommandHandlers.HandleUpdate)
                .WithParsed<DeleteOptions>(CommandHandlers.HandleDelete)
                .WithParsed<ImportOptions>(CommandHandlers.HandleImport)
                .WithParsed<ExportOptions>(CommandHandlers.HandleExport)
                .WithParsed<RenameOptions>(CommandHandlers.HandleRename)
                .WithParsed<ExistsOptions>(CommandHandlers.HandleExists)
                .WithParsed<ReadOptions>(CommandHandlers.HandleRead)
                .WithParsed<ListOptions>(CommandHandlers.HandleList);
        }
    }
}