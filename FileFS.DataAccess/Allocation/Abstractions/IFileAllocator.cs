namespace FileFS.DataAccess.Allocation.Abstractions
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
        /// <param name="isNewFile">If true, space for additional entry descriptor will be checked to be allocated, otherwise false.</param>
        /// <returns>Cursor that points to a allocated space for a file data.</returns>
        Cursor AllocateFile(int dataSize, bool isNewFile);
    }
}