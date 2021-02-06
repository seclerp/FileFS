namespace FileFs.DataAccess
{
    public struct CursorRange
    {
        public readonly Cursor Begin;
        public readonly Cursor End;

        public CursorRange(Cursor begin, Cursor end)
        {
            Begin = begin;
            End = end;
        }
    }
}