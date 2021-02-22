using CommandLine;
using FileFS.Cli.Options;

namespace FileFS.Cli
{
    /// <summary>
    /// Entry point container class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Array containing command line arguments.</param>
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                    InitOptions,
                    CreateDirectoryOptions,
                    CreateOptions,
                    UpdateOptions,
                    DeleteOptions,
                    ImportOptions,
                    ExportOptions,
                    RenameOptions,
                    MoveOptions,
                    CopyOptions,
                    ExistsOptions,
                    ReadOptions,
                    ListOptions
                >(args)
                .WithParsed<InitOptions>(CommandHandlers.HandleInit)
                .WithParsed<CreateDirectoryOptions>(CommandHandlers.HandleCreateDirectory)
                .WithParsed<CreateOptions>(CommandHandlers.HandleCreate)
                .WithParsed<UpdateOptions>(CommandHandlers.HandleUpdate)
                .WithParsed<DeleteOptions>(CommandHandlers.HandleDelete)
                .WithParsed<ImportOptions>(CommandHandlers.HandleImport)
                .WithParsed<ExportOptions>(CommandHandlers.HandleExport)
                .WithParsed<RenameOptions>(CommandHandlers.HandleRename)
                .WithParsed<MoveOptions>(CommandHandlers.HandleMove)
                .WithParsed<CopyOptions>(CommandHandlers.HandleCopy)
                .WithParsed<ExistsOptions>(CommandHandlers.HandleExists)
                .WithParsed<ReadOptions>(CommandHandlers.HandleRead)
                .WithParsed<ListOptions>(CommandHandlers.HandleList);
        }
    }
}