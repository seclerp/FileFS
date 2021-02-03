namespace FileFS.Managers
{
    public interface IFileFsManager
    {
        void Create(string fileName, byte[] contentBytes);

        byte[] Read(string fileName);

        void Rename(string oldFilename, string newFilename);

        void Delete(string fileName);

        bool Exists(string fileName);
    }
}