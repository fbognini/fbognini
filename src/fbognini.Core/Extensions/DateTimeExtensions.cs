using System;

namespace fbognini.Core.Extensions
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


        /// <summary>
        /// Converts the given date value to epoch time.
        /// </summary>
        public static long ToEpochTime(this DateTime dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
            var ts = ticks / TimeSpan.TicksPerSecond;
            return ts;
        }

        /// <summary>
        /// Converts the given date value to epoch time.
        /// </summary>
        public static long ToEpochTime(this DateTimeOffset dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
            var ts = ticks / TimeSpan.TicksPerSecond;
            return ts;
        }

        /// <summary>
        /// Converts the given epoch time to a <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> kind.
        /// </summary>
        public static DateTime ToDateTimeFromEpoch(this long intDate)
        {
            var timeInTicks = intDate * TimeSpan.TicksPerSecond;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(timeInTicks);
        }

        /// <summary>
        /// Converts the given epoch time to a UTC <see cref="DateTimeOffset"/>.
        /// </summary>
        public static DateTimeOffset ToDateTimeOffsetFromEpoch(this long intDate)
        {
            var timeInTicks = intDate * TimeSpan.TicksPerSecond;
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddTicks(timeInTicks);
        }

        public static DateTime ToDateTimeFromUnixTimeStamp(this string unixTimeStamp)
        {
            return long.Parse(unixTimeStamp).ToDateTimeFromUnixTimeStamp();
        }

        public static DateTime ToDateTimeFromUnixTimeStamp(this long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).LocalDateTime;
        }

        public static DateTimeOffset ToDateTimeOffsetFromUnixTimeStamp(this string unixTimeStamp)
        {
            return long.Parse(unixTimeStamp).ToDateTimeOffsetFromUnixTimeStamp();
        }

        public static DateTimeOffset ToDateTimeOffsetFromUnixTimeStamp(this long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
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
        public static TimeSpan Round(this TimeSpan ts,
            RoundingDirection Direction,
            int MinutePrecision)
        {
            if (Direction == RoundingDirection.Up)
            {
                return TimeSpan.FromMinutes(
                    MinutePrecision * Math.Ceiling(ts.TotalMinutes / MinutePrecision));
            }

            if (Direction == RoundingDirection.Down)
            {
                return TimeSpan.FromMinutes(
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
