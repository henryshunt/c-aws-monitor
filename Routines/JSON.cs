using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_AWSMonitor.Routines
{
    public static class JSON
    {
        public class AWSInfo
        {
            public string Name { get; set; }
            public string TimeZone { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Elevation { get; set; }
        }

        public class ChartPoint
        {
            [JsonProperty("x")]
            public long X { get; set; }
            [JsonProperty("y")]
            public double? Y { get; set; }
        }

        public class EnvReport
        {
            public string Time { get; set; }
            public double? EncT { get; set; }
            public double? CPUT { get; set; }
        }

        public class Report
        {
            public string Time { get; set; }
            public double? AirT { get; set; }
            public double? ExpT { get; set; }
            public double? RelH { get; set; }
            public double? DewP { get; set; }
            public double? WSpd { get; set; }
            public int? WDir { get; set; }
            public double? WGst { get; set; }
            public int? SunD { get; set; }
            public int? SunD_PHr { get; set; }
            public double? Rain { get; set; }
            public double? Rain_PHr { get; set; }
            public double? StaP { get; set; }
            public double? MSLP { get; set; }
            public double? StaP_PTH { get; set; }
            public double? ST10 { get; set; }
            public double? ST30 { get; set; }
            public double? ST00 { get; set; }
        }
    }
}
