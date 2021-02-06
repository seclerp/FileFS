using System;
using System.Text;
using FileFS.Cli.Constants;
using FileFS.Cli.Extensions;
using FileFS.Cli.Options;
using FileFS.DataAccess;
using FileFS.DataAccess.Serializers;

namespace FileFS.Cli
{
    internal class CommandHandlers
    {
        internal static void HandleInit(InitOptions initOptions)
        {
            CommandHandlerHelper.TryExecute(initOptions, options =>
            {
                var serializer = new FilesystemDescriptorSerializer();
                var logger = CommandHandlerHelper.CreateLogger(options.IsDebug);
                var manager = new StorageInitializer(serializer, logger);
                manager.Initialize(options.Instance, options.Size, options.PathLength);
            });
        }

        internal static void HandleCreate(CreateOptions createOptions)
        {
            CommandHandlerHelper.TryExecute(createOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var contentBytes = Encoding.UTF8.GetBytes(options.Content);
                client.Create(options.FileName, contentBytes);
            });
        }

        internal static void HandleUpdate(UpdateOptions updateOptions)
        {
            CommandHandlerHelper.TryExecute(updateOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var contentBytes = Encoding.UTF8.GetBytes(options.Content);
                client.Update(options.FileName, contentBytes);

                if (options.ForceOptimize)
                {
                    client.ForceOptimize();
                }
            });
        }

        internal static void HandleDelete(DeleteOptions deleteOptions)
        {
            CommandHandlerHelper.TryExecute(deleteOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Delete(options.FileName);

                if (options.ForceOptimize)
                {
                    client.ForceOptimize();
                }
            });
        }

        internal static void HandleImport(ImportOptions importOptions)
        {
            CommandHandlerHelper.TryExecute(importOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Import(options.ImportPath, options.FileName);
            });
        }

        internal static void HandleExport(ExportOptions exportOptions)
        {
            CommandHandlerHelper.TryExecute(exportOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Export(options.FileName, options.ExportPath);
            });
        }

        internal static void HandleRename(RenameOptions renameOptions)
        {
            CommandHandlerHelper.TryExecute(renameOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Rename(options.OldFileName, options.NewFileName);
            });
        }

        internal static void HandleExists(ExistsOptions existsOptions)
        {
            CommandHandlerHelper.TryExecute(existsOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var exists = client.Exists(options.FileName);
                Console.WriteLine(exists);
            });
        }

        internal static void HandleRead(ReadOptions readOptions)
        {
            CommandHandlerHelper.TryExecute(readOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var contentBytes = client.ReadContent(options.FileName);
                var content = Encoding.UTF8.GetString(contentBytes);
                Console.WriteLine(content);
            });
        }

        internal static void HandleList(ListOptions listOptions)
        {
            CommandHandlerHelper.TryExecute(listOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var allEntries = client.ListFiles();

                if (options.IsDetailedView)
                {
                    Console.WriteLine("{0, -21}{1, 9}{2, 21}{3, 21}", "NAME", "SIZE", "CREATED ON", "UPDATED ON");
                    foreach (var entryInfo in allEntries)
                    {
                        Console.WriteLine(
                            "{0, -21}{1, 9}{2, 21}{3, 21}",
                            entryInfo.FileName.Clip(17),
                            ((long)entryInfo.Size).FormatBytesSize(),
                            entryInfo.CreatedOn.ToString(CliConstants.DateTimeFormat),
                            entryInfo.UpdatedOn.ToString(CliConstants.DateTimeFormat));
                    }
                }
                else
                {
                    foreach (var entryInfo in allEntries)
                    {
                        Console.WriteLine(entryInfo.FileName);
                    }
                }
            });
        }
    }
}