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
        public static void Seek(this Stream stream, Cursor cursor)
        {
            stream.Seek(cursor.Offset, cursor.Origin);
        }
    }
}