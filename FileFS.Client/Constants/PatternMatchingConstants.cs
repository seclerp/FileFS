namespace FileFS.Client.Constants
{
    /// <summary>
    /// Class containing pattern matching constants.
    /// </summary>
    public class PatternMatchingConstants
    {
        /// <summary>
        /// Pattern that represents valid filename for FileFS storage.
        /// </summary>
        public static readonly string ValidFilename = @"^(\/[\p{L}\-. ]+)*$";
    }
}