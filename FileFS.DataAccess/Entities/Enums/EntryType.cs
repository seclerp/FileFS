namespace FileFS.DataAccess.Entities.Enums
{
    /// <summary>
    /// Enum that represents different entry types stored in entry descriptor.
    /// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Entry describes file.
        /// </summary>
        File = 1,

        /// <summary>
        /// Entry describes directory.
        /// </summary>
        Directory = 2,
    }
}