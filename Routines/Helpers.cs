using NodaTime;
using System;

namespace C_AWSMonitor.Routines
{
    public static class Helpers
    {
        public static DateTime UTCToLocal(DateTime utc)
        {
            DateTimeZone dtz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
                Properties.Settings.Default.AWSTimeZone);

            Instant utc2 = Instant.FromDateTimeUtc(utc);
            return utc2.InZone(dtz).ToDateTimeUnspecified();
        }

        public static DateTime UTCToLocal(long utc)
        {
            DateTimeZone dtz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
                Properties.Settings.Default.AWSTimeZone);

            Instant utc2 = Instant.FromUnixTimeSeconds(utc);
            return utc2.InZone(dtz).ToDateTimeUnspecified();
        }

        public static Tuple<DateTime, DateTime> GetDayBounds(DateTime time)
        {
            // Return start and end of time
            DateTime start = new DateTime(time.Year, time.Month, time.Day);
            DateTime end = start.AddDays(1);

            return new Tuple<DateTime, DateTime>(start, end);
        }

        public static string DegreesToCompass(int degrees)
        {
            if (degrees >= 338 || degrees < 23) return "N";
            else if (degrees >= 23 && degrees < 68) return "NE";
            else if (degrees >= 68 && degrees < 113) return "E";
            else if (degrees >= 113 && degrees < 158) return "SE";
            else if (degrees >= 158 && degrees < 203) return "S";
            else if (degrees >= 203 && degrees < 248) return "SW";
            else if (degrees >= 248 && degrees < 293) return "W";
            else if (degrees >= 293 && degrees < 338) return "NW";
            else return "N";
        }
    }
}
