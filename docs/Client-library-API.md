# Client library API

## `IStorageInitializer`

---
### `void Initialize(int fileSize, int fileNameLength)`

Performs initialization of new FileFS storage
- `fileSize`: Size that will be used as reserved for new FileFS storage
- `fileNameLength`: Maximum length for filenames
---

## `IFileFsClient`

---
### `void Initialize(int fileSize, int fileNameLength)`
Performs initialization of new FileFS storage
- `fileSize`: Size that will be used as reserved for new FileFS storage
- `fileNameLength`: Maximum length for filenames

---
### `void Create(string fileName)`
Creates new file with empty data
- `fileName`: Name of a file to create

---
### `void Create(string fileName, byte[] data)`
Creates new file with given data
- `fileName`: Name of a file to create
- `data`: Data bytes

---
### `void Create(string fileName, Stream sourceStream, int length)`
Creates new file with given data
- `fileName`: Name of a file to create
- `sourceStream`: Source stream of bytes
- `length`: Length of data to be read in bytes

---
### `void Update(string fileName, byte[] newData)`
Updates existing file using new data
- `fileName`: Name of a file to update
- `newData`: New data bytes

---
### `void Update(string fileName, Stream sourceStream, int length)`
Updates existing file using new data
- `fileName`: Name of a file to update
- `sourceStream`: Source stream of bytes
- `length`: Length of data to be read in bytes

---
### `byte[] Read(string fileName)`
Reads data of existing file
- `fileName`: Name of a file to read

Returns: File's data bytes

---
### `void Read(string fileName, Stream destinationStream)`
Reads data of existing file
- `fileName`: Name of a file to read
- `destinationStream`: Destination stream of bytes

---
### `void Rename(string currentFilename, string newFilename)`
Change name of existing file
- `currentFilename`: Current name of file
- `newFilename`: New name of a file

---
### `void Delete(string fileName)`
Deletes existing file
- `fileName`: Name of a file to delete

---
### `void Import(string externalPath, string fileName)`
Imports external file into FileFS storage
- `externalPath`: Path to external file to import
- `fileName`: Name of a new file in FileFS storage

---
### `void Export(string fileName, string externalPath)`
Exports file from FileFS storage to new external file
- `fileName`: Name of a existing file in FileFS storage
- `externalPath`: Path to new external file to export

---
### `void Exists(string fileName)`
Returns true if file with given name exists in FileFS storage, otherwise false
- `fileName`: Name of a file to check

Returns: True if file with given name exists in FileFS storage, otherwise false

---
### `void Exists(string fileName)`
Returns true if file with given name exists in FileFS storage, otherwise false
- `fileName`: Name of a file to check

Returns: True if file with given name exists in FileFS storage, otherwise false

---
### `IEnumerable<FileEntryInfo> ListFiles()`
Returns all files inside FileFS storage

Returns: Enumerable that represents all files inside FileFS storage

---
### `int ForceOptimize()`
Manually calls optimizer to optimize FileFS storage space

Returns: Count of bytes that was optimized

---
## `FileEntryInfo`
Properties:
- `string FileName`: Name of the file
- `int Size`: File size in bytes
- `DateTime CreatedOn`: DateTime instance that represents time when file was created
- `DateTime UpdatedOn`: DateTime instance that represents time when file was updated last time