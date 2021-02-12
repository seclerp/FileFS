## CLI usage guide

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

For detailed syntax of each command use `--help`.
