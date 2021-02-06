using System.IO;

namespace FileFS.DataAccess
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