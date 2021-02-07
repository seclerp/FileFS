namespace FileFS.DataAccess.Memory.Abstractions
{
    /// <summary>
    /// Abstraction responsible for optimizing of used space in FileFS storage.
    /// </summary>
    public interface IStorageOptimizer
    {
        /// <summary>
        /// Performs optimization process.
        /// </summary>
        void Optimize();
    }
}