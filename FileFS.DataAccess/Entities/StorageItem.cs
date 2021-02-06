namespace FileFS.DataAccess.Entities
{
    public struct StorageItem<TValue>
        where TValue : struct
    {
        public readonly TValue Value;
        public readonly Cursor Cursor;

        public StorageItem(ref TValue value, ref Cursor cursor)
        {
            Value = value;
            Cursor = cursor;
        }
    }
}