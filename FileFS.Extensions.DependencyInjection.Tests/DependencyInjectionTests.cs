using FileFS.Client.Abstractions;
using FileFS.DataAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

// Missing XML comment for publicly visible type or member...
#pragma warning disable 1591

// Elements should be documented
#pragma warning disable SA1600

namespace FileFS.Extensions.DependencyInjection.Tests
{
    /// <summary>
    /// Tests for IoC container services resolving.
    /// </summary>
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddFileFsClient_ShouldSuccessfullyResolveClient()
        {
            // Arrange
            var logger = new LoggerConfiguration().CreateLogger();
            var services = new ServiceCollection()
                .AddSingleton<ILogger>(logger)
                .AddFileFsClient("stub.storage");

            // Act
            void Act()
            {
                services.BuildServiceProvider().GetRequiredService<IFileFsClient>();
                services.BuildServiceProvider().GetRequiredService<IStorageInitializer>();
            }

            // Assert
            Assert.Null(Record.Exception(Act));
        }
    }
}