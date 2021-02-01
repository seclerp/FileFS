using FileFS.Api.Abstractions;

namespace FileFS.Api
{
    public class FileFsClient : IFileFsClient
    {
        public FileFsClient(string fileFsPath, int fileSize)
        {

        }

        public void Create(string fileName, byte[] content)
        {
            throw new System.NotImplementedException();
        }

        public void Update(string fileName, byte[] newContent)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public byte[] Read(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public bool Exists(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void Rename(string oldName, string newName)
        {
            throw new System.NotImplementedException();
        }
    }
}