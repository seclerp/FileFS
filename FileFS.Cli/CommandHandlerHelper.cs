using System;
using FileFS.Cli.Options;
using FileFS.Client;
using FileFS.Client.Abstractions;
using FileFS.DataAccess.Exceptions;
using Serilog;

namespace FileFS.Cli
{
    /// <summary>
    /// Class that contains helper methods used by command handlers.
    /// </summary>
    internal static class CommandHandlerHelper
    {
        /// <summary>
        /// Creates configured logger instance.
        /// </summary>
        /// <param name="isDebug">If true, all logs would be logged, otherwise only Warning and higher levels.</param>
        /// <returns>Configured logger instance.</returns>
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

        /// <summary>
        /// Creates ready to use <see cref="IFileFsClient"/> instance.
        /// </summary>
        /// <param name="options">Contains base command options, used to configure client.</param>
        /// <returns>Ready to use <see cref="IFileFsClient"/> instance.</returns>
        internal static IFileFsClient CreateClient(BaseOptions options)
        {
            return FileFsClientFactory.Create(options.Instance, CreateLogger(options.IsDebug));
        }

        /// <summary>
        /// Wraps given action execution into try/catch block, executes it and formats known errors into console.
        /// </summary>
        /// <param name="options">Command options instance.</param>
        /// <param name="action">Action to be executed.</param>
        /// <typeparam name="TOptions">Type of a command options.</typeparam>
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