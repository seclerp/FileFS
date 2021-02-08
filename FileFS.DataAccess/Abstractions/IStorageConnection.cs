using System.IO;

namespace FileFS.DataAccess.Abstractions
{
    /// <summary>
    /// Abstraction that represents low level FileFS storage connection.
    /// </summary>
    public interface IStorageConnection
    {
        /// <summary>
        /// Performs write of specified data.
        /// </summary>
        /// <param name="cursor">Cursor that points to destination space to write data.</param>
        /// <param name="data">Data to be written.</param>
        void PerformWrite(Cursor cursor, byte[] data);

        /// <summary>
        /// Performs write of specified data.
        /// </summary>
        /// <param name="cursor">Cursor that points to destination space to write data.</param>
        /// <param name="length">Length of the data to be written in bytes.</param>
        /// <param name="sourceStream">Source stream of data.</param>
        void PerformWrite(Cursor cursor, int length, Stream sourceStream);

        /// <summary>
        /// Performs read of the data.
        /// </summary>
        /// <param name="cursor">Cursor that points to the data in storage.</param>
        /// <param name="length">Length of the data to be read.</param>
        /// <returns>Data fof specified length that was read from the storage.</returns>
        byte[] PerformRead(Cursor cursor, int length);

        /// <summary>
        /// Performs read of the data.
        /// </summary>
        /// <param name="cursor">Cursor that points to the data in storage.</param>
        /// <param name="length">Length of the data to be read.</param>
        /// <param name="destinationStream">Stream where data should be written after read.</param>
        void PerformRead(Cursor cursor, int length, Stream destinationStream);

        /// <summary>
        /// Perform copying of the data.
        /// </summary>
        /// <param name="sourceCursor">Cursor that points to an existing data.</param>
        /// <param name="destinationCursor">Cursor that points to a new destination.</param>
        /// <param name="length">Length of the data to be copied.</param>
        void PerformCopy(Cursor sourceCursor, Cursor destinationCursor, int length);

        /// <summary>
        /// Returns reserved size of a FileFS storage.
        /// </summary>
        /// <returns>Reserved size of a FileFS storage.</returns>
        long GetSize();
    }
}