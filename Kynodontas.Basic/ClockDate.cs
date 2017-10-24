using System;

namespace Kynodontas.Basic
{
    /// <summary>
    /// Resolve struct Date_Time shortcomings
    /// </summary>
    public class ClockDate
    {
        public static string TimezoneIdPst = "SA Pacific Standard Time";
        public static ClockDate UtcNow { get { return new ClockDate(DateTime.UtcNow); } }
        public static DateTime MinValue { get { return DateTime.MinValue; } }
        public static DateTimeOffset MaxValue { get { return DateTimeOffset.MaxValue; } }

        public DateTime TimeDate { get; }
        public DateTime ToPstDate
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(TimeDate, timeZonePst);
            }
        }

        public double ToUnixTicks
        {
            get
            {
                // https://stackoverflow.com/a/1016886/6227407
                var initialUnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan ts = new TimeSpan(TimeDate.Ticks - initialUnixTime.Ticks);
                return ts.TotalMilliseconds;
            }
        }

        private TimeZoneInfo timeZonePst = TimeZoneInfo.FindSystemTimeZoneById(TimezoneIdPst);

        /// <summary>
        /// Create object from Utc DateTime
        /// </summary>
        public ClockDate(DateTime dateTime)
        {
            if(dateTime.Kind != DateTimeKind.Utc)
            {
                throw new Exception("appDeveloper: DateTimeKind is not Utc");
            }
            TimeDate = dateTime;
        }

        public ClockDate(DateTime dateTime, string dateTimeKind)
        {
            DateTime currentDate;
            if (dateTimeKind == "Local" && dateTime.Kind == DateTimeKind.Local)
            {
                currentDate = dateTime.ToUniversalTime();
            }
            else
            {
                throw new Exception("appDeveloper: Parameters are not accepted");
            }

            TimeDate = currentDate;
        }

        /// <summary>
        /// Insert PST hours and date to create object
        /// </summary>
        public ClockDate(int year, int month, int day, int hour, int minute)
        {
            var dateTime = new DateTime(year, month, day, hour, minute, 00, DateTimeKind.Unspecified);
            TimeDate = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZonePst);
        }

        public ClockDate AddSeconds(int value)
        {
            return new ClockDate(TimeDate.AddSeconds(value));
        }

        public ClockDate AddMinutes(double value)
        {
            return new ClockDate(TimeDate.AddMinutes(value));
        }

        public ClockDate AddHours(double value)
        {
            return new ClockDate(TimeDate.AddHours(value));
        }

        public ClockDate AddDays(double value)
        {
            return new ClockDate(TimeDate.AddDays(value));
        }

        public ClockDate AddMonths(int value)
        {
            return new ClockDate(TimeDate.AddMonths(value));
        }

        public ClockDate AddYears(int value)
        {
            return new ClockDate(TimeDate.AddYears(value));
        }
    }
}
