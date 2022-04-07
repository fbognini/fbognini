using System;

namespace fbognini.Core.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Get the day of week from Monday (1) to Sunday (7)
        /// </summary>
        public static int GetDayOfWeekISO8601(this DateTime dt)
        {
            return ((int)dt.DayOfWeek + 6) % 7 + 1;
        }

        public static DateTime FromUnixTimeStampToDateTime(this string unixTimeStamp)
        {
            return FromUnixTimeStampToDateTime(long.Parse(unixTimeStamp));
        }

        public static DateTime FromUnixTimeStampToDateTime(this long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).LocalDateTime;
        }
    }

    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Rounds a TimeSpan based on the provided values.
        /// </summary>
        /// <param name="ts">The extension target.</param>
        /// <param name="Direction">The direction in which to round.</param>
        /// <param name="MinutePrecision">The precision to round to.</param>
        /// <returns>A new TimeSpan based on the provided values.</returns>
        public static System.TimeSpan Round(this System.TimeSpan ts,
            RoundingDirection Direction,
            int MinutePrecision)
        {
            if (Direction == RoundingDirection.Up)
            {
                return System.TimeSpan.FromMinutes(
                    MinutePrecision * Math.Ceiling(ts.TotalMinutes / MinutePrecision));
            }

            if (Direction == RoundingDirection.Down)
            {
                return System.TimeSpan.FromMinutes(
                    MinutePrecision * Math.Floor(ts.TotalMinutes / MinutePrecision));
            }

            // Really shouldn't be able to get here...
            return ts;
        }
    }

    /// <summary>
    /// Rounding direction used in rounding operations. 
    /// </summary>
    public enum RoundingDirection
    {
        /// <summary>
        /// Round up.
        /// </summary>
        Up,
        /// <summary>
        /// Round down.
        /// </summary>
        Down
    }
}
