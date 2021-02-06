namespace FileFS.DataAccess.Abstractions
{
    public interface IStorageInitializer
    {
        void Initialize(string fileName, int fileSize, int fileNameLength);
    }
}