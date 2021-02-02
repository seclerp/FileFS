namespace FileFs.DataAccess.Repositories.Abstractions
{
    public interface IFileDataRepository
    {
        byte[] Read(int offset, int length);

        void Write(byte[] data, int offset);
    }
}