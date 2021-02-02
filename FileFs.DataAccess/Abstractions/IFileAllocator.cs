namespace FileFs.DataAccess
{
    public interface IFileAllocator
    {
        Cursor AllocateFile(int dataSize);
    }
}