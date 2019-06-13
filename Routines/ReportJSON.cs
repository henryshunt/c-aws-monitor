namespace C_AWSMonitor.Routines
{
    public class ReportJSON
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
