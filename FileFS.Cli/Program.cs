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
            var manager = new FileFsInitializer(serializer);
            manager.Initialize(options.Instance, options.Size, options.PathLength, FileFsStorageVersion);
        }

        private static void HandleCreate(CreateOptions options)
        {
            var client = new FileFsClient(options.Instance);
            var contentBytes = Encoding.UTF8.GetBytes(options.Content);
            client.Create(options.FileName, contentBytes);
        }

        private static void HandleDelete(DeleteOptions options)
        {
            var client = new FileFsClient(options.Instance);
            client.Delete(options.FileName);
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
                    DeleteOptions,
                    RenameOptions,
                    ExistsOptions,
                    ReadOptions,
                    ListOptions
                >(args)
                .WithParsed<InitOptions>(HandleInit)
                .WithParsed<CreateOptions>(HandleCreate)
                .WithParsed<DeleteOptions>(HandleDelete)
                .WithParsed<RenameOptions>(HandleRename)
                .WithParsed<ExistsOptions>(HandleExists)
                .WithParsed<ReadOptions>(HandleRead)
                .WithParsed<ListOptions>(HandleList);
        }
    }
}