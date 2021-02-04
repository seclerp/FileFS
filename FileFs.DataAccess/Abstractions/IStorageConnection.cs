namespace FileFs.DataAccess.Abstractions
{
    public interface IStorageConnection
    {
        void PerformWrite(Cursor cursor, byte[] data);
        byte[] PerformRead(Cursor cursor, int length);
        void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length);
        long GetSize();
    }
}