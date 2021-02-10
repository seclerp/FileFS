using System.IO;

namespace FileFS.DataAccess.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Stream"/> type.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Sets the position within the <see cref="Stream"/> instance using information provided by <see cref="Cursor"/> instance.
        /// </summary>
        /// <param name="stream">Instance of <see cref="Stream"/>.</param>
        /// <param name="cursor">Instance of <see cref="Cursor"/>.</param>
        public static void Seek(this Stream stream, in Cursor cursor)
        {
            stream.Seek(cursor.Offset, cursor.Origin);
        }

        /// <summary>
        /// Reads data from source stream and writes it to destination stream using fixed size buffer.
        /// </summary>
        /// <param name="source">Source <see cref="Stream"/> instance.</param>
        /// <param name="destination">Destination <see cref="Stream"/> instance.</param>
        /// <param name="length">Overall amount of bytes to be read.</param>
        /// <param name="bufferSize">Size of intermediate buffer.</param>
        public static void WriteBuffered(this Stream source, Stream destination, int length, int bufferSize)
        {
            var buffer = new byte[bufferSize >= length ? length : bufferSize];
            var bytesProcessed = 0;

            while (bytesProcessed < length)
            {
                var bytesRead = source.Read(buffer, 0, length - bytesProcessed >= buffer.Length ? buffer.Length : length - bytesProcessed);
                destination.Write(buffer, 0, bytesRead);

                // End of stream
                if (bytesRead < buffer.Length)
                {
                    break;
                }

                bytesProcessed += bytesRead;
            }
        }
    }
}