using System.IO;

namespace FileFS.DataAccess
{
    /// <summary>
    /// Type that represents cursor in memory.
    /// </summary>
    public readonly struct Cursor
    {
        /// <summary>
        /// Offset in bytes from origin.
        /// </summary>
        public readonly int Offset;

        /// <summary>
        /// Origin point to count bytes from.
        /// </summary>
        public readonly SeekOrigin Origin;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cursor"/> struct.
        /// </summary>
        /// <param name="offset">Offset in bytes from origin.</param>
        /// <param name="origin">Origin point to count bytes from.</param>
        public Cursor(int offset, SeekOrigin origin)
        {
            Offset = offset;
            Origin = origin;
        }
    }
}