using System;
using System.IO;
using FileFS.DataAccess.Entities.Abstractions;

namespace FileFS.DataAccess.Entities
{
    /// <summary>
    /// Type that represents file and its streamed data representation.
    /// </summary>
    public readonly struct StreamedFileEntry : IFileEntry, IDisposable
    {
        /// <summary>
        /// File data stream.
        /// </summary>
        public readonly Stream DataStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamedFileEntry"/> struct.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="dataStream">File data stream.</param>
        /// <param name="dataLength">Length of data in bytes.</param>
        public StreamedFileEntry(string fileName, Stream dataStream, int dataLength)
        {
            FileName = fileName;
            DataStream = dataStream;
            DataLength = dataLength;
        }

        /// <inheritdoc />
        public string FileName { get; }

        /// <inheritdoc />
        public int DataLength { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            DataStream?.Dispose();
        }
    }
}