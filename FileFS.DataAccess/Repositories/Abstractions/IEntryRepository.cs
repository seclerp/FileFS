namespace FileFS.DataAccess.Repositories.Abstractions
{
    /// <summary>
    /// Abstraction for base entry repository.
    /// </summary>
    public interface IEntryRepository
    {
        /// <summary>
        /// Renames entry with given name.
        /// </summary>
        /// <param name="currentEntryName">Current name of a file to rename.</param>
        /// <param name="newEntryName">New name of a file to rename.</param>
        void Rename(string currentEntryName, string newEntryName);

        /// <summary>
        /// Deletes entry with given name.
        /// </summary>
        /// <param name="entryName">Name of a file to delete.</param>
        void Delete(string entryName);

        /// <summary>
        /// Returns true if entry with given name exists, otherwise false.
        /// </summary>
        /// <param name="entryName">Name of a file to check.</param>
        /// <returns>True if entry with given name exists, otherwise false.</returns>
        bool Exists(string entryName);

        /// <summary>
        /// Moves entry from one absolute path to another.
        /// </summary>
        /// <param name="fromName">Initial name of the entry.</param>
        /// <param name="toName">Target name of the entry.</param>
        void Move(string fromName, string toName);
    }
}