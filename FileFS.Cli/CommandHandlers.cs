using System;
using System.Text;
using FileFS.Cli.Constants;
using FileFS.Cli.Extensions;
using FileFS.Cli.Options;
using FileFS.DataAccess;
using FileFS.DataAccess.Serializers;

namespace FileFS.Cli
{
    /// <summary>
    /// Class that contains methods used to handle commands.
    /// </summary>
    internal class CommandHandlers
    {
        /// <summary>
        /// Method that handles "init" command.
        /// </summary>
        /// <param name="initOptions">Options passed to the command.</param>
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

        /// <summary>
        /// Method that handles "create" command.
        /// </summary>
        /// <param name="createOptions">Options passed to the command.</param>
        internal static void HandleCreate(CreateOptions createOptions)
        {
            CommandHandlerHelper.TryExecute(createOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var contentBytes = Encoding.UTF8.GetBytes(options.Content);
                client.Create(options.FileName, contentBytes);
            });
        }

        /// <summary>
        /// Method that handles "update" command.
        /// </summary>
        /// <param name="updateOptions">Options passed to the command.</param>
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

        /// <summary>
        /// Method that handles "delete" command.
        /// </summary>
        /// <param name="deleteOptions">Options passed to the command.</param>
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

        /// <summary>
        /// Method that handles "import" command.
        /// </summary>
        /// <param name="importOptions">Options passed to the command.</param>
        internal static void HandleImport(ImportOptions importOptions)
        {
            CommandHandlerHelper.TryExecute(importOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Import(options.ImportPath, options.FileName);
            });
        }

        /// <summary>
        /// Method that handles "export" command.
        /// </summary>
        /// <param name="exportOptions">Options passed to the command.</param>
        internal static void HandleExport(ExportOptions exportOptions)
        {
            CommandHandlerHelper.TryExecute(exportOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Export(options.FileName, options.ExportPath);
            });
        }

        /// <summary>
        /// Method that handles "rename" command.
        /// </summary>
        /// <param name="renameOptions">Options passed to the command.</param>
        internal static void HandleRename(RenameOptions renameOptions)
        {
            CommandHandlerHelper.TryExecute(renameOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                client.Rename(options.OldFileName, options.NewFileName);
            });
        }

        /// <summary>
        /// Method that handles "exists" command.
        /// </summary>
        /// <param name="existsOptions">Options passed to the command.</param>
        internal static void HandleExists(ExistsOptions existsOptions)
        {
            CommandHandlerHelper.TryExecute(existsOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var exists = client.Exists(options.FileName);
                Console.WriteLine(exists);
            });
        }

        /// <summary>
        /// Method that handles "read" command.
        /// </summary>
        /// <param name="readOptions">Options passed to the command.</param>
        internal static void HandleRead(ReadOptions readOptions)
        {
            CommandHandlerHelper.TryExecute(readOptions, options =>
            {
                var client = CommandHandlerHelper.CreateClient(options);
                var contentBytes = client.Read(options.FileName);
                var content = Encoding.UTF8.GetString(contentBytes);
                Console.WriteLine(content);
            });
        }

        /// <summary>
        /// Method that handles "list" command.
        /// </summary>
        /// <param name="listOptions">Options passed to the command.</param>
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