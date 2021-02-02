using System;
using FileFs.DataAccess.Abstractions;
using FileFs.DataAccess.Entities;

namespace FileFS.Api
{
    public class FileAllocator
    {
        private readonly IFileFsConnection _connection;

        public FileAllocator(IFileFsConnection connection)
        {
            _connection = connection;
        }

        public FileDescriptor AllocateFile(int length)
        {
            throw new NotImplementedException();
        }
    }
}