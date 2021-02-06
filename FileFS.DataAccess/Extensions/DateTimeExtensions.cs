using System;

namespace FileFS.DataAccess.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTime(this DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime);
            return offset.ToUnixTimeSeconds();
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return offset.DateTime;
        }
    }
}