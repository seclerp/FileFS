using System;
using FileFS.Cli.Options;
using FileFS.Client;
using FileFS.Client.Abstractions;
using FileFS.DataAccess.Exceptions;
using Serilog;

namespace FileFS.Cli
{
    internal static class CommandHandlerHelper
    {
        internal static ILogger CreateLogger(bool isDebug)
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

        internal static IFileFsClient CreateClient(BaseOptions options)
        {
            return FileFsClientFactory.Create(options.Instance, CreateLogger(options.IsDebug));
        }

        internal static void TryExecute<TOptions>(TOptions options, Action<TOptions> action)
            where TOptions : BaseOptions
        {
            try
            {
                action(options);
            }

            // Catch here only known exception, other should lead to unexpected exit (as defined by default)
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
    }
}