﻿namespace FileFs.DataAccess.Abstractions
{
    public interface IFileAllocator
    {
        Cursor AllocateFile(int dataSize);
    }
}