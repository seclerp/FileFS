using System;
using System.Text;
using CommandLine;
using FileFS.Api;
using FileFS.Cli.CommandLineOptions;
using FileFs.DataAccess;
using FileFs.DataAccess.Serializers;

namespace FileFS.Cli
{
    class Program
    {
        // Increase version when layout of FileFS storage changes
        private static readonly int FileFsStorageVersion = 1;

        private static void HandleInit(InitOptions options)
        {
            var serializer = new FilesystemDescriptorSerializer();
            var manager = new StorageInitializer(serializer);
            manager.Initialize(options.Instance, options.Size, options.PathLength, FileFsStorageVersion);
        }

        private static void HandleCreate(CreateOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var contentBytes = Encoding.UTF8.GetBytes(options.Content);
            client.Create(options.FileName, contentBytes);
        }

        private static void HandleUpdate(UpdateOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var contentBytes = Encoding.UTF8.GetBytes(options.Content);
            client.Update(options.FileName, contentBytes);

            if (options.ForceOptimize)
            {
                client.ForceOptimize();
            }
        }

        private static void HandleDelete(DeleteOptions options)
        {
            var client = new FileFsClient(options.Instance);
            client.Delete(options.FileName);

            if (options.ForceOptimize)
            {
                client.ForceOptimize();
            }
        }

        private static void HandleImport(ImportOptions options)
        {
            var client = new FileFsClient(options.Instance);
            client.Import(options.ImportPath, options.FileName);
        }

        private static void HandleExport(ExportOptions options)
        {
            var client = new FileFsClient(options.Instance);
            client.Export(options.FileName, options.ExportPath);
        }

        private static void HandleRename(RenameOptions options)
        {
            var client = new FileFsClient(options.Instance);
            client.Rename(options.OldFileName, options.NewFileName);
        }

        private static void HandleExists(ExistsOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var exists = client.Exists(options.FileName);
            Console.WriteLine(exists);
        }

        private static void HandleRead(ReadOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var contentBytes = client.Read(options.FileName);
            var content = Encoding.UTF8.GetString(contentBytes);
            Console.WriteLine(content);
        }

        private static void HandleList(ListOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var allEntries = client.List();
            Console.WriteLine("{0, -20}{1, 5}", "NAME", "SIZE");
            foreach (var entryInfo in allEntries)
            {
                Console.WriteLine("{0, -20}{1, 5}", entryInfo.Path, $"{entryInfo.Size}B");
            }
        }

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
                .WithParsed<InitOptions>(HandleInit)
                .WithParsed<CreateOptions>(HandleCreate)
                .WithParsed<UpdateOptions>(HandleUpdate)
                .WithParsed<DeleteOptions>(HandleDelete)
                .WithParsed<ImportOptions>(HandleImport)
                .WithParsed<ExportOptions>(HandleExport)
                .WithParsed<RenameOptions>(HandleRename)
                .WithParsed<ExistsOptions>(HandleExists)
                .WithParsed<ReadOptions>(HandleRead)
                .WithParsed<ListOptions>(HandleList);
        }
    }
}