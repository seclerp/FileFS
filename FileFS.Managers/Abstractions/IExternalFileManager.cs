namespace FileFS.Managers
{
    public interface IExternalFileManager
    {
        void Write(string externalFileName, byte[] data);
        byte[] Read(string externalFileName);
    }
}