using System.IO;

namespace FileFS.DataAccess.Extensions
{
    /// <summary>
    /// Extensions for <see cref="BinaryReader"/>.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Read bytes those represents Guid instance.
        /// </summary>
        /// <param name="reader">Instance of <see cref="BinaryReader"/>.</param>
        /// <returns>Byte array that represents Guid instance.</returns>
        public static byte[] ReadGuidBytes(this BinaryReader reader)
        {
            return reader.ReadBytes(16);
        }
    }
}