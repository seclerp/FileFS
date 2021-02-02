using System.IO;

namespace FileFs.DataAccess
{
    public struct Cursor
    {
        public readonly int Offset;
        public readonly SeekOrigin Origin;

        public Cursor(int offset, SeekOrigin origin)
        {
            Offset = offset;
            Origin = origin;
        }
    }
}