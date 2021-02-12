### Library

First of all you need to add reference to `FileFS.Client` library to your project.

You also need to install `Serilog` package and some sinks (`Serilog.Sinks.Console` for example), because `FileFS.Client` uses Serilog logger for logging purposes.

You also need to prepare Serilog logger instance, for example:

```csharp
var logger =
    new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();
```

### Storage initializartion

If you don't already have storage, you should create one. To do that you should use `StorageInitializer`:

```csharp
var storageInitializer = StorageInitializerFactory.Create("filefs.storage", logger);
storageInitializer.Initialize(10000, 256);
```

### FileFsClient object

After that you could create `IFileFsClient` via standard factory:

```csharp
var client = FileFsClientFactory.Create("filefs.storage", logger);
```

You could also pass options object to customize some values:

```csharp
var clientOptions = new FileFsClientOptions
{
    ByteBufferSize = 2048,
    EnableTransactions = true,
};
var client = FileFsClientFactory.Create("filefs.storage", clientOptions, logger);
```

### Dependency injection

If your application uses dependency injection, there is extension library `FileFS.Extensions.DependencyInjection` that has extensions methods for using with `IServiceCollection`:

```csharp
services.AddFileFsClient("path-to.storage");
```

Or with options:

```csharp
var clientOptions = new FileFsClientOptions
{
    ByteBufferSize = 2048,
    EnableTransactions = true,
};
services.AddFileFsClient("path-to.storage", options);
```

After such configuration you could receive `IFileFsClient` instance (and of course other dependencies if you wish) via `IServiceProvider` or using ASP.NET Core constructor injection.

For example usage of client library look into CLI source code. There is also **[client library API documentation](Client-library-API)**.