namespace FileFS.Api.Models
{
    public struct FileDescriptor
    {
        public static readonly int BytesWithoutFilename = 8;

        public FileDescriptor(string fileName, int offset, int length)
        {
            FileNameLength = fileName.Length;
            FileName = fileName;
            Offset = offset;
            Length = length;
        }

        public readonly int FileNameLength;

        public readonly string FileName;

        public readonly int Offset;

        public readonly int Length;
    }
}