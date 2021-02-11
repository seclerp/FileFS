using FileFS.DataAccess.Exceptions;

namespace FileFS.Client.Configuration
{
    /// <summary>
    /// Type for <see cref="FileFsClientOptions"/> validation.
    /// </summary>
    public static class FileFsClientOptionsValidator
    {
        /// <summary>
        /// Validates <see cref="FileFsClientOptions"/> instance.
        /// </summary>
        /// <param name="options">Instance of <see cref="FileFsClientOptions"/>.</param>
        /// <exception cref="ArgumentNonValidException">Throws when ByteBufferSize is less or equals 0.</exception>
        public static void Validate(FileFsClientOptions options)
        {
            if (options.ByteBufferSize < 1)
            {
                throw new ArgumentNonValidException($"{nameof(options.ByteBufferSize)} should be bigger than 0, got {options.ByteBufferSize}");
            }
        }
    }
}