using FileFS.Client.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

namespace FileFS.Extensions.DependencyInjection.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void ServiceCollection_AddFileFsClient_ShouldNotThrowException()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILogger>(logger);
            serviceCollection.AddFileFsClient("stub.storage");
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var _ = serviceProvider.GetService<IFileFsClient>();
        }
    }
}