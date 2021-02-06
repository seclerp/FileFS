using System;
using System.Text;
using CommandLine;
using FileFS.Api;
using FileFS.Api.Abstractions;
using FileFS.Cli.CommandLineOptions;
using FileFs.DataAccess;
using FileFs.DataAccess.Exceptions;
using FileFs.DataAccess.Serializers;
using Serilog;

namespace FileFS.Cli
{
    internal class Program
    {

        // Increase version when layout of FileFS storage changes
        private static readonly int FileFsStorageVersion = 1;

        private static ILogger CreateLogger(bool isDebug)
        {
            var configuration = new LoggerConfiguration();
            if (isDebug)
            {
                configuration = configuration.MinimumLevel.Information();
            }
            else
            {
                configuration = configuration.MinimumLevel.Warning();
            }

            return configuration
                .WriteTo.Console()
                .CreateLogger();
        }

        private static IFileFsClient CreateClient(BaseOptions options)
        {
            return new FileFsClient(options.Instance, CreateLogger(options.IsDebug));
        }

        private static void SafeExecute<TOptions>(TOptions options, Action<TOptions> action)
            where TOptions : BaseOptions
        {
            try
            {
                action(options);
            }

            // Catch here only known exception
            catch (FileFsException ex)
            {
                if (options.IsDebug)
                {
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ForegroundColor = currentColor;
                }
                else
                {
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR: ");
                    Console.ForegroundColor = currentColor;
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void HandleInit(InitOptions initOptionas)
        {
            SafeExecute(initOptionas, options =>
            {
                var serializer = new FilesystemDescriptorSerializer();
                var logger = CreateLogger(options.IsDebug);
                var manager = new StorageInitializer(serializer, logger);
                manager.Initialize(options.Instance, options.Size, options.PathLength, FileFsStorageVersion);
            });
        }

        private static void HandleCreate(CreateOptions createOptions)
        {
            SafeExecute(createOptions, options =>
            {
                var client = CreateClient(options);
                var contentBytes = Encoding.UTF8.GetBytes(options.Content);
                client.Create(options.FileName, contentBytes);
            });
        }

        private static void HandleUpdate(UpdateOptions updateOptions)
        {
            SafeExecute(updateOptions, options =>
            {
                var client = CreateClient(options);
                var contentBytes = Encoding.UTF8.GetBytes(options.Content);
                client.Update(options.FileName, contentBytes);

                if (options.ForceOptimize)
                {
                    client.ForceOptimize();
                }
            });
        }

        private static void HandleDelete(DeleteOptions deleteOptions)
        {
            SafeExecute(deleteOptions, options =>
            {
                var client = CreateClient(options);
                client.Delete(options.FileName);

                if (options.ForceOptimize)
                {
                    client.ForceOptimize();
                }
            });
        }

        private static void HandleImport(ImportOptions importOptions)
        {
            SafeExecute(importOptions, options =>
            {
                var client = CreateClient(options);
                client.Import(options.ImportPath, options.FileName);
            });
        }

        private static void HandleExport(ExportOptions exportOptions)
        {
            SafeExecute(exportOptions, options =>
            {
                var client = CreateClient(options);
                client.Export(options.FileName, options.ExportPath);
            });
        }

        private static void HandleRename(RenameOptions renameOptions)
        {
            SafeExecute(renameOptions, options =>
            {
                var client = CreateClient(options);
                client.Rename(options.OldFileName, options.NewFileName);
            });
        }

        private static void HandleExists(ExistsOptions existsOptions)
        {
            SafeExecute(existsOptions, options =>
            {
                var client = CreateClient(options);
                var exists = client.Exists(options.FileName);
                Console.WriteLine(exists);
            });
        }

        private static void HandleRead(ReadOptions readOptions)
        {
            SafeExecute(readOptions, options =>
            {
                var client = CreateClient(options);
                var contentBytes = client.Read(options.FileName);
                var content = Encoding.UTF8.GetString(contentBytes);
                Console.WriteLine(content);
            });
        }

        private static void HandleList(ListOptions listOptions)
        {
            SafeExecute(listOptions, options =>
            {
                var client = CreateClient(options);
                var allEntries = client.List();
                Console.WriteLine("{0, -20}{1, 5}", "NAME", "SIZE");
                foreach (var entryInfo in allEntries)
                {
                    Console.WriteLine("{0, -20}{1, 5}", entryInfo.FileName, $"{entryInfo.Size}B");
                }
            });
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