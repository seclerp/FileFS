namespace FileFS.Client.Abstractions
{
    public interface IExternalFileManager
    {
        void Write(string externalFileName, byte[] data);
        byte[] Read(string externalFileName);
    }
}