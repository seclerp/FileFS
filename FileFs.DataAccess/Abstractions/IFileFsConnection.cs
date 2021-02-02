using System.IO;

namespace FileFs.DataAccess.Abstractions
{
    public interface IFileFsConnection
    {
        void PerformWrite(int offset, byte[] data, SeekOrigin origin);
        byte[] PerformRead(int offset, int length, SeekOrigin origin);
    }
}