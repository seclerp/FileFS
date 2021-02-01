using System.IO;
using FileFS.Api.Abstractions;
using FileFS.Api.Models;
using FileFS.Api.Serializers.Abstractions;

namespace FileFS.Api
{
    public interface IFileFsManager
    {
        void CreateEmpty(string fileName, int fileSize, int fileNameLength, int version);

        Stream Open(string fileName);
    }

    public class FileFsManager : IFileFsManager
    {
        private readonly ISerializer<FilesystemDescriptor> _serializer;

        public FileFsManager(ISerializer<FilesystemDescriptor> serializer)
        {
            _serializer = serializer;
        }

        public void CreateEmpty(string fileName, int fileSize, int fileNameLength, int version)
        {
            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            stream.SetLength(fileSize);
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);

            var fileSystemDescriptor =
                new FilesystemDescriptor(0, 0, fileNameLength + FileDescriptor.BytesWithoutFilename, 1);

            _serializer.WriteTo(stream, fileSystemDescriptor);
        }

        public Stream Open(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
        }
    }
}