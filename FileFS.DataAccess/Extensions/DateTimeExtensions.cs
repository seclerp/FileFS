using System;

namespace FileFS.DataAccess.Extensions
{
    /// <summary>
    /// Extensions for <see cref="DateTime"/> type.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns Unix timestamp representation of given <see cref="DateTime"/> instance.
        /// </summary>
        /// <param name="dateTime">Instance of <see cref="DateTime"/>.</param>
        /// <returns>Unix timestamp representation of given <see cref="DateTime"/> instance.</returns>
        public static long ToUnixTime(this DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime);
            return offset.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> instance that represents given Unix timestamp.
        /// </summary>
        /// <param name="unixTime">Unix timestamp value.</param>
        /// <returns><see cref="DateTime"/> instance that represents given Unix timestamp.</returns>
        public static DateTime FromUnixTime(this long unixTime)
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return offset.DateTime;
        }
    }
}