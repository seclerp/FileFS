using System.IO;
using System.Text;
using FileFS.Api.Models;
using FileFS.Api.Serializers.Abstractions;

namespace FileFS.Api.Serializers
{
    public class FilesystemDescriptorSerializer : ISerializer<FilesystemDescriptor>
    {
        public FilesystemDescriptor ReadFrom(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var filesDataLength = reader.ReadInt32();
            var fileDescriptorsCount = reader.ReadInt32();
            var fileDescriptorLength = reader.ReadInt32();
            var version = reader.ReadInt32();

            return new FilesystemDescriptor(filesDataLength, fileDescriptorsCount, fileDescriptorLength, version);
        }

        public void WriteTo(Stream stream, FilesystemDescriptor model)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(model.FilesDataLength);
            writer.Write(model.FileDescriptorsCount);
            writer.Write(model.FileDescriptorLength);
            writer.Write(model.Version);
        }
    }
}