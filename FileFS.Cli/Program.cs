using System.Text;
using FileFS.Api;
using FileFs.DataAccess;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;
using FileFs.DataAccess.Serializers;
using FileFs.DataAccess.Serializers.Abstractions;

namespace FileFS.Cli
{
    class Program
    {
        private static void CreateNew(ISerializer<FilesystemDescriptor> filesystemDescriptorSerializer, string fileName)
        {
            var manager = new FileFsInitializer(filesystemDescriptorSerializer);
            manager.Initialize(fileName, 10 * 1024 * 1024, 256, 1);
        }

        private static IFileFsConnection Open(string fileName)
        {
            var connection = new FileFsConnection(fileName);
            return connection;
        }

        static void Main(string[] args)
        {
            var fileName = "filefs";
            var filesystemSerializer = new FilesystemDescriptorSerializer();

            CreateNew(filesystemSerializer, fileName);

            var client = new FileFsClient(fileName);

            var newFileContent = Encoding.UTF8.GetBytes("Hello World!");
            var newFileName = "hello-world";

            client.Create(newFileName, newFileContent);

            var fileContentBytes = client.Read(newFileName);
            var fileContent = Encoding.UTF8.GetString(fileContentBytes);
        }
    }
}