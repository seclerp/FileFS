using System.IO;
using FileFS.Api;
using FileFS.Api.Models;
using FileFS.Api.Serializers;

namespace FileFS.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var filesystemSerializer = new FilesystemDescriptorSerializer();
            var manager = new FileFsManager(filesystemSerializer);
            manager.CreateEmpty("filefs", 10 * 1024 * 1024, 256, 1);
            using var stream = manager.Open("filefs");
            stream.Seek(-FilesystemDescriptor.BytesTotal, SeekOrigin.End);
            var filesystemDescriptor = filesystemSerializer.ReadFrom(stream);

            var fileDescriptorSerializer = new FileDescriptorSerializer(filesystemDescriptor);
            var fileDescriptor = new FileDescriptor("example", 0, 0);
            stream.Seek(0, SeekOrigin.Begin);
            fileDescriptorSerializer.WriteTo(stream, fileDescriptor);
            stream.Seek(0, SeekOrigin.Begin);
            var deserializedFileDescriptor = fileDescriptorSerializer.ReadFrom(stream);
        }
    }
}