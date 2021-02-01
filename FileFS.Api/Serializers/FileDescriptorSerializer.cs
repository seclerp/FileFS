using System.IO;
using System.Text;
using FileFS.Api.Models;
using FileFS.Api.Serializers.Abstractions;

namespace FileFS.Api.Serializers
{
    public class FileDescriptorSerializer : ISerializer<FileDescriptor>
    {
        private readonly FilesystemDescriptor _filesystemDescriptor;

        public FileDescriptorSerializer(FilesystemDescriptor filesystemDescriptor)
        {
            _filesystemDescriptor = filesystemDescriptor;
        }

        public FileDescriptor ReadFrom(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var stringLength = reader.ReadInt32();
            var fileName = Encoding.UTF8.GetString(reader.ReadBytes(stringLength));
            stream.Seek(_filesystemDescriptor.FileDescriptorLength - stringLength, SeekOrigin.Current);
            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();

            return new FileDescriptor(fileName, offset, length);
        }

        public void WriteTo(Stream stream, FileDescriptor model)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(model.FileNameLength);
            var fileNameBytes = Encoding.UTF8.GetBytes(model.FileName);
            writer.Write(fileNameBytes);
            writer.Seek(_filesystemDescriptor.FileDescriptorLength - Encoding.UTF8.GetByteCount(model.FileName), SeekOrigin.Current);
        }
    }
}