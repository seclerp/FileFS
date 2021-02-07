namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents data in FileFS storage with cursor that points on it in memory.
    /// </summary>
    /// <typeparam name="TValue">Type of the stored value.</typeparam>
    public readonly struct StorageItem<TValue>
        where TValue : struct
    {
        /// <summary>
        /// Value stored in FileFS storage.
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        /// Cursor that points on data stored in FileFS storage.
        /// </summary>
        public readonly Cursor Cursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageItem{TValue}"/> struct.
        /// </summary>
        /// <param name="value">Value stored in FileFS storage.</param>
        /// <param name="cursor">Cursor that points on data stored in FileFS storage.</param>
        public StorageItem(ref TValue value, ref Cursor cursor)
        {
            Value = value;
            Cursor = cursor;
        }
    }
}