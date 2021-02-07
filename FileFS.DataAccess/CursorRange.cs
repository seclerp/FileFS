namespace FileFS.DataAccess
{
    /// <summary>
    /// Type that represents range from one cursor to another.
    /// </summary>
    public readonly struct CursorRange
    {
        /// <summary>
        /// Begin of the range.
        /// </summary>
        public readonly Cursor Begin;

        /// <summary>
        /// End of the range.
        /// </summary>
        public readonly Cursor End;

        /// <summary>
        /// Initializes a new instance of the <see cref="CursorRange"/> struct.
        /// </summary>
        /// <param name="begin">Begin of the range.</param>
        /// <param name="end">End of the range.</param>
        public CursorRange(in Cursor begin, in Cursor end)
        {
            Begin = begin;
            End = end;
        }
    }
}