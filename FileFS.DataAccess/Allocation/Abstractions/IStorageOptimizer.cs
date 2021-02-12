namespace FileFS.DataAccess.Allocation.Abstractions
{
    /// <summary>
    /// Abstraction responsible for optimizing of used space in FileFS storage.
    /// </summary>
    public interface IStorageOptimizer
    {
        /// <summary>
        /// Performs optimization process.
        /// </summary>
        /// <returns>Optimized bytes count.</returns>
        int Optimize();
    }
}