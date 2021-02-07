using System.IO;
using System.Text;
using FileFS.DataAccess.Entities;
using FileFS.DataAccess.Serializers.Abstractions;

namespace FileFS.DataAccess.Serializers
{
    /// <summary>
    /// Filesystem descriptor serializer implementation.
    /// </summary>
    public class FilesystemDescriptorSerializer : ISerializer<FilesystemDescriptor>
    {
        /// <inheritdoc />
        public FilesystemDescriptor FromBuffer(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            var filesDataLength = reader.ReadInt32();
            var fileDescriptorsCount = reader.ReadInt32();
            var fileDescriptorLength = reader.ReadInt32();

            return new FilesystemDescriptor(filesDataLength, fileDescriptorsCount, fileDescriptorLength);
        }

        /// <inheritdoc />
        public byte[] ToBuffer(FilesystemDescriptor model)
        {
            var buffer = new byte[FilesystemDescriptor.BytesTotal];
            using var stream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            writer.Write(model.FilesDataLength);
            writer.Write(model.FileDescriptorsCount);
            writer.Write(model.FileDescriptorLength);

            return buffer;
        }
    }
}