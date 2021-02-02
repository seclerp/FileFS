namespace FileFs.DataAccess.Abstractions
{
    public interface IFileFsInitializer
    {
        void Initialize(string fileName, int fileSize, int fileNameLength, int version);
    }
}