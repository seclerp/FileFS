# FileFS

![Build and Test](https://github.com/seclerp/FileFS/workflows/Build%20and%20Test/badge.svg)

Simple single file-based filesystem.

## Overview

**FileFS** stores file files information inside single file, called **storage**.

Features:
- Basic file operations: create, read, update, delete, rename, exists, list all files
- Import and export of external files (with buffering)
- Defragmentation of storage space (manually or automatically when there is not enough space)
- Exclusive access support across multiple client instances
- Debugging using verbose logging

You could find latest library and CLI release in **[Releases](https://github.com/seclerp/FileFS/releases)** section.

## Documentation

- **[Client library getting started](docs/Client-library-getting-started)**
- **[Client library API](docs/Client-library-API.md)**
- **[CLI usage guide](docs/CLI-usage-guide.md)**
- **[Architecture overview](docs/Architecture-overview.md)**

## Limitations

- Maximum supported size of file is 2 147 483 647 bytes. This is due to Int32 and some additional challenges with streaming and in-memory copying (see [#15](https://github.com/seclerp/FileFS/issues/15))).
- Exclusive access works only inside 1 machine, so FileFS storage file stored at remote network will lose such feature.
- Filename are stored in fixed-sized memory blocks, so they need to have fixed maximum size.
- Defragmentation of storage with very large files may be very slow

## Build

`FileFS.Cli` project is a **.NET Core 3.1** application, and `FileFS.Client` project is **.NET Standard 2.1** compatible library.

For building both of them you should have **.NET Core SDK 3.1** installed.

To build whole solution use:

`> dotnet build`

From root repository folder.

Main dependencies:

- `CommandLineParser` (CLI)
- `Serilog` (DataAccess, Client, CLI)
- `xUnit` (Tests)
- `Moq` (Tests)
- `StyleCop.Analysers` (All projects)
- `Microsoft.Extensions.DependencyInjection` (Tests and Extensions.DependencyInjection)

## Tests

There are 3 test projects:
- `FileFS.DataAccess.Tests`: contains tests for low level part of a library
- `FileFS.Client.Tests`: contains tests for library's API itself
- `FileFS.Extensions.DependencyInjection.Tests`: tests for dependency injection project

To run all tests from solution use:

`> dotnet test`

## Fields for improvement

- Fix Int32 size limitation for large files support (see [#15](https://github.com/seclerp/FileFS/issues/15)
- Add folders support (see [#8](https://github.com/seclerp/FileFS/issues/8))
- Improve memory management, especially byte arrays by using Span<T> (see [#1](https://github.com/seclerp/FileFS/issues/1))
