namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction that represents allocator for memory space used to contain file data.
    /// </summary>
    public interface IFileAllocator
    {
        /// <summary>
        /// Returns cursor that points to a allocated space for a file data.
        /// </summary>
        /// <param name="dataSize">Required size of a file data to allocate.</param>
        /// <returns>Cursor that points to a allocated space for a file data.</returns>
        Cursor AllocateFile(int dataSize);
    }
}