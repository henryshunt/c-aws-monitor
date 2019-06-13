using System;

namespace C_AWSMonitor.Routines
{
    public static class Helpers
    {
        public static Tuple<DateTime, DateTime> GetDayBounds(DateTime utc)
        {
            // Return start and end of UTC date
            DateTime start = new DateTime(utc.Year, utc.Month, utc.Day);
            DateTime end = start.AddDays(1);

            return new Tuple<DateTime, DateTime>(start, end);
        }
    }
}
