namespace FileFS.Client.Constants
{
    /// <summary>
    /// Constants for synchronization and inter-process locking.
    /// </summary>
    public static class SynchronizationConstants
    {
        /// <summary>
        /// Template for mutex name.
        /// </summary>
        public static readonly string MutexNameTemplate = "FileFsStorageMutexId-{0}";
    }
}