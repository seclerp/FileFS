using System;

namespace FileFS.DataAccess.Entities
{
    public struct FileEntryInfo
    {
        public readonly string FileName;

        public readonly int Size;

        public readonly DateTime CreatedOn;

        public readonly DateTime UpdatedOn;

        public FileEntryInfo(string fileName, int size, DateTime createdOn, DateTime updatedOn)
        {
            FileName = fileName;
            Size = size;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }
    }
}