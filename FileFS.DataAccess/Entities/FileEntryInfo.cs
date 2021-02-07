using System;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents meta information about the file.
    /// </summary>
    public readonly struct FileEntryInfo
    {
        /// <summary>
        /// Name of the file.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public readonly int Size;

        /// <summary>
        /// DateTime instance that represents time when file was created.
        /// </summary>
        public readonly DateTime CreatedOn;

        /// <summary>
        /// DateTime instance that represents time when file was updated last time.
        /// </summary>
        public readonly DateTime UpdatedOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntryInfo"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">File size in bytes.</param>
        /// <param name="createdOn">DateTime instance that represents time when file was created.</param>
        /// <param name="updatedOn">DateTime instance that represents time when file was updated last time.</param>
        public FileEntryInfo(string fileName, int size, DateTime createdOn, DateTime updatedOn)
        {
            FileName = fileName;
            Size = size;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }
    }
}