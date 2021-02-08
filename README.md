# FileFS

[Build status here]

Simple single file-based filesystem.

## Overview

**FileFS** stores file files information inside single file, called **storage**.

Features:
- Basic file operations: create, read, update, delete, rename, exists, list all files
- Import and export of external files (with buffering)
- Defragmentation of storage space (manually or automatically when there is not enough space)
- Well documented .NET client library
- Ready-to-use CLI application for standalone storage manipulation
- Debugging using verbose logging

You could find latest library and CLI release in Releases section.

## How to use

### Command Line Interface

CLI exposes full set of features included in FileFS client library.

All command available through `--help` option:

```
> ./filefs --help

filefs 1.0.0
Copyright (C) 2021 Andrew Rublyov

  init       Initialize new storage instance of FileFS.

  create     Create new file inside FileFS storage.

  update     Update file content inside FileFS storage.

  delete     Deletes file from FileFS storage.

  import     Imports existing file in your filesystem to a new file inside FileFS storage.

  export     Exports file from FileFS storage to a new file in your filesystem.

  rename     Rename file in FileFS storage.

  exists     Check that file is exists in FileFS storage.

  read       Read contents of file inside FileFS storage.

  list       List files inside FileFS storage.

  help       Display more information on a specific command.

  version    Display version information.
```

Before working with storage, you should create it via `init` command:

```
> ./filefs init
```
This will create `filefs.storage` file with default storage size in your working directory.

Each command has its own help information, for example `init` command help:

```
> ./filefs init --help

filefs 1.0.0
Copyright (C) 2021 Andrew Rublyov

  -s, --size           (Default: 10485760) Size of a newly created storage in bytes.

  -n, --name-length    (Default: 256) Maximum length of name of the file in bytes.

  -i, --instance       (Default: filefs.storage) Set filename for FileFS file (instance) to work with.

  --debug              (Default: false) Enable detailed logging during execution of the command.

  --help               Display this help screen.

  --version            Display version information.
```

After creating storage, you could start working with it, for example:

```
> ./filefs create hello-world "Hello, World!"

> ./filefs read hello-world

Hello, World!

> ./filefs import image.jpg my-avatar.jpg

> ./filefs list --details

NAME                      SIZE           CREATED ON           UPDATED ON
hello-world                13B  08.02.2021 00:40:08  08.02.2021 00:40:08
my-avatar.jpg            5.7MB  08.02.2021 00:40:21  08.02.2021 00:40:21
```

### Library

First of all you need to add reference to `FileFS.Client` library to your project.

You also need to install `Serilog` package and some sinks (`Serilog.Sinks.Console` for example), because `FileFS.Client` uses Serilog logger for logging purposes.

After that you could use `IFileFsClient` via standard factory:

```csharp
using System;
using FileFS.Client;
using Serilog;

private class Program
{
    private static void Main(string[] args)
    {
        var logger =
            new LoggingConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        var client = FileFsClientFactory.Create("path-to.storage", logger);
        
        var data = Encoding.UTF8.GetBytes("Example content of a newly created file");
        client.Create("example.txt", data);
        
        client.Delete("example.txt");
    }
}

```

... or via dependency injection (for example in ASP.NET Core app):

```csharp
using FileFS.Client;
using FileFS.Extensions.DependencyInjection;
using Serilog;

public class Startup
{
    // ...
    
    public void ConfigureServices(IServiceCollection services)
    {
        // FileFS also depends on Serilog ILogger
        var logger =
            new LoggingConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        
        services.AddSingleton(logger);
        services.AddFileFsClient("path-to.storage");
    }
}
```

... or creating `FileFsClient` manually.
This is the most customizable option, use it if your want to provide custom implementation of internal services.

For more examples of library usage see `FileFS.Cli` project.

## Build

CLI project is an **.NET Core 3.1** and library project is **.NET Standard 2.1** compliant library.

For building both of them you should have **.NET Core SDK 3.1** or higher installed.

To build whole solution use:

`> dotnet build`

From root repository folder.

## Tests

There are 3 test projects:
- `FileFS.DataAccess.Tests`: contains tests for low level part of a library
- `FileFS.Client.Tests`: contains tests for library's API itself
- `FileFS.Extensions.DependencyInjection.Tests`: tests for dependency injection project

To run all tests from solution use:

`> dotnet test`

## Dependencies

- `CommandLineParser` (CLI)
- `Serilog` (DataAccess, Client, CLI)
- `xUnit` (Tests)
- `Moq` (Tests)
- `StyleCop.Analysers` (All projects)

## Architecture

TODO