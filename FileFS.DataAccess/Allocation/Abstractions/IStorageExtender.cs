namespace FileFS.DataAccess.Allocation.Abstractions
{
    /// <summary>
    /// Abstraction that represents storage extender.
    /// </summary>
    public interface IStorageExtender
    {
        /// <summary>
        /// Extends FileFS storage to a new size.
        /// </summary>
        /// <param name="newSize">New size of FileFS storage. Should be greater that current one.</param>
        void Extend(long newSize);
    }
}