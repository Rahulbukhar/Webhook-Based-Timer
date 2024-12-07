

using System;

namespace NoSqlDataAccess.Common.Extensions
{
    /// <summary>
    /// This class provides extension methods for DateTime operations.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a given DateTime into a Unix timestamp.
        /// </summary>
        /// <param name="value">Input <see cref="DateTime"/>.</param>
        /// <returns>Unix timestamp format</returns>
        public static long ToUnixTimestamp(this DateTime value)
        {
            return (long) Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        /// <summary>
        /// Converts unix epoch timestamp to DateTime.
        /// </summary>
        /// <param name="epoch">The unix timestamp.</param>
        /// <returns>An instance of <see cref="DateTime"/>.</returns>
        public static DateTime ToDateTime(long epoch)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(epoch).ToLocalTime();
            return dateTime;
        }
    }
}
