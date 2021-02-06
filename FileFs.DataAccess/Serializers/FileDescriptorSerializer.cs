using System.IO;
using System.Text;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Repositories.Abstractions;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFs.DataAccess.Serializers
{
    public class FileDescriptorSerializer : ISerializer<FileDescriptor>
    {
        private readonly IFilesystemDescriptorAccessor _filesystemDescriptorAccessor;

        public FileDescriptorSerializer(IFilesystemDescriptorAccessor filesystemDescriptorAccessor)
        {
            _filesystemDescriptorAccessor = filesystemDescriptorAccessor;
        }

        public FileDescriptor FromBuffer(byte[] buffer)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            using var stream = new MemoryStream(buffer);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            var stringLength = reader.ReadInt32();
            var fileNameBytes = reader.ReadBytes(stringLength);
            var fileName = Encoding.UTF8.GetString(fileNameBytes);
            stream.Seek(filesystemDescriptor.FileDescriptorLength - stringLength - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();

            return new FileDescriptor(fileName, offset, length);
        }

        public byte[] ToBuffer(FileDescriptor model)
        {
            var filesystemDescriptor = _filesystemDescriptorAccessor.Value;
            var fileNameBytes = Encoding.UTF8.GetBytes(model.FileName);
            var buffer = new byte[filesystemDescriptor.FileDescriptorLength];
            using var stream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            writer.Write(model.FileNameLength);
            writer.Write(fileNameBytes);
            writer.Seek(filesystemDescriptor.FileDescriptorLength - fileNameBytes.Length - FileDescriptor.BytesWithoutFilename, SeekOrigin.Current);
            writer.Write(model.DataOffset);
            writer.Write(model.DataLength);

            return buffer;
        }
    }
}