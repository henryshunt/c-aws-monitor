using Newtonsoft.Json;
using NodaTime;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using C_AWSMonitor.Routines;

namespace C_AWSMonitor
{
    public partial class DataPage : Page
    {
        private bool IsLoading = false;

        public DateTime DataTime { get; private set; }
        private DateTime LastUpdated;
        private ChartViewModel ChartModel = new ChartViewModel();

        public event EventHandler DataDownloadStarted;
        public event EventHandler DataDownloadCompleted;

        public DataPage()
        {
            InitializeComponent();
            PlotAirT.DataContext = ChartModel;
        }

        public void LoadData(bool forceRefresh)
        {
            if (IsLoading) return;

            // Don't download new data if current is less than a minute old
            if (!forceRefresh)
            {
                if (LastUpdated != DateTime.MinValue)
                {
                    TimeSpan difference = DateTime.UtcNow - LastUpdated;
                    if (difference.TotalMinutes < 1) return;
                }
            }

            try
            {
                Thread worker = new Thread(delegate ()
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    { DataDownloadStarted?.Invoke(this, new EventArgs()); });

                    DateTime dataTime = DateTime.UtcNow;
                    string utc = dataTime.ToString("yyyy-MM-dd'T'HH-mm-00");

                    // Download and deserialise report for current time
                    string reportUrl = Path.Combine(Properties.Settings.Default
                        .DataEndpoint + "/", "data/now.php?time=" + utc);

                    string reportData = new WebClient().DownloadString(reportUrl);
                    ReportJSON reportJson = JsonConvert.DeserializeObject<ReportJSON>(
                        reportData);

                    // Recalibrate time to returned record time
                    dataTime = DateTime.Parse(reportJson.Time);
                    dataTime = DateTime.SpecifyKind(dataTime, DateTimeKind.Utc);
                    utc = dataTime.ToString("yyyy-MM-dd'T'HH-mm-00");


                    // Download and deserialise environment report for current time
                    string reportEnvUrl = Path.Combine(Properties.Settings.Default
                        .DataEndpoint + "/", "data/station.php?time=" + utc + "&abs=1");

                    string reportEnvData = new WebClient().DownloadString(reportEnvUrl);
                    EnvReportJSON envReportJson = JsonConvert.DeserializeObject<EnvReportJSON>(
                        reportEnvData);


                    // Download and deserialise chart data for current day
                    string graphUrl = Path.Combine(Properties.Settings.Default
                        .DataEndpoint + "/", "data/graph-day.php?time=" + utc
                        + "&fields=AirT");

                    string graphData = new WebClient().DownloadString(graphUrl)
                        .Replace("[[", "[").Replace("]]", "]");
                    List<ChartPointJSON> graphJson = JsonConvert.DeserializeObject<List<
                        ChartPointJSON>>(graphData);

                    LastUpdated = dataTime;
                    DataTime = dataTime;

                    // Display downloaded data in the interface
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        if (reportJson.AirT != null)
                            LabelAirT.Content = ((double)reportJson.AirT).ToString("0.0") + "°C";
                        else LabelAirT.Content = "No Data";

                        // Add points to chart
                        ChartModel.ClearPoints();
                        DateTimeZone dtz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
                            Properties.Settings.Default.AWSTimeZone);

                        foreach (var item in graphJson)
                        {
                            if (item.y != null)
                            {
                                Instant utc2 = Instant.FromUnixTimeSeconds(item.x);
                                ChartModel.AddPoint(utc2.InZone(dtz).ToDateTimeUnspecified(),
                                    (double)item.y);
                            }
                        }

                        if (reportJson.RelH != null)
                            LabelRelH.Content = ((double)reportJson.RelH).ToString("0.0") + "%";
                        else LabelRelH.Content = "No Data";

                        if (reportJson.WSpd != null)
                            LabelWSpd.Content = ((double)reportJson.WSpd).ToString("0.0;0") + " mph";
                        else LabelWSpd.Content = "No Data";

                        if (reportJson.WDir != null)
                            LabelWDir.Content = reportJson.WDir.ToString() + "°";
                        else LabelWDir.Content = "No Data";

                        if (reportJson.WGst != null)
                            LabelWGst.Content = ((double)reportJson.WGst).ToString("0.0") + " mph";
                        else LabelWGst.Content = "No Data";

                        if (reportJson.SunD_PHr != null)
                        {
                            TimeSpan ts = TimeSpan.FromSeconds((double)reportJson.SunD_PHr);
                            LabelSunDPHr.Content = new DateTime(ts.Ticks).ToString("HH:mm:ss");
                        }
                        else LabelSunDPHr.Content = "No Data";

                        if (reportJson.Rain_PHr != null)
                        {
                            LabelRainPHr.Content =
                                ((double)reportJson.Rain_PHr).ToString("0.00") + " mm";
                        }
                        else LabelRainPHr.Content = "No Data";

                        if (reportJson.MSLP != null)
                            LabelMSLP.Content = ((double)reportJson.MSLP).ToString("0.0") + " hPa";
                        else LabelMSLP.Content = "No Data";

                        if (reportJson.StaP_PTH != null)
                        {
                            LabelStaPPTH.Content =
                                ((double)reportJson.StaP_PTH).ToString("+0.0;-0.0") + " hPa";
                        }
                        else LabelStaPPTH.Content = "No Data";

                        if (reportJson.ST10 != null)
                            LabelST10.Content = ((double)reportJson.ST10).ToString("0.0") + "°C";
                        else LabelST10.Content = "No Data";

                        if (reportJson.ST30 != null)
                            LabelST30.Content = ((double)reportJson.ST30).ToString("0.0") + "°C";
                        else LabelST30.Content = "No Data";

                        if (reportJson.ST00 != null)
                            LabelST00.Content = ((double)reportJson.ST00).ToString("0.0") + "°C";
                        else LabelST00.Content = "No Data";

                        if (envReportJson.CPUT != null)
                            LabelCPUT.Content = ((double)envReportJson.CPUT).ToString("0.0") + "°C";
                        else LabelCPUT.Content = "No Data";

                        // Calculate chart boundaries
                        var bounds = Helpers.GetDayBounds(dataTime);
                        DateTimeAxisAirTX.Minimum = DateTimeAxis.ToDouble(bounds.Item1);
                        DateTimeAxisAirTX.Maximum = DateTimeAxis.ToDouble(bounds.Item2);
                        SetYAxisSettings();
                        PlotAirT.InvalidatePlot();

                        DataDownloadCompleted?.Invoke(this, new EventArgs());
                    });
                });

                worker.IsBackground = true;
                worker.Start();
            }
            catch { DataDownloadCompleted?.Invoke(this, new EventArgs()); }
        }

        private void SetYAxisSettings()
        {
            double min = ChartModel.GetMinimum();
            double max = ChartModel.GetMaximum();
            double range = max - min;

            // Set axis step size and range based on data range
            if (range <= 2)
            {
                LinearAxisAirTY.MajorStep = 0.5;
                LinearAxisAirTY.Minimum = min - 0.5;
                LinearAxisAirTY.Maximum = max + 0.5;
            }
            else if (range > 2 && range <= 5)
            {
                LinearAxisAirTY.MajorStep = 1;
                LinearAxisAirTY.Minimum = min - 0.5;
                LinearAxisAirTY.Maximum = max + 0.5;
            }
            else if (range > 5 && range <= 10)
            {
                LinearAxisAirTY.MajorStep = 2;
                LinearAxisAirTY.Minimum = min - 1;
                LinearAxisAirTY.Maximum = max + 1;
            }
            else if (range > 10 && range <= 25)
            {
                LinearAxisAirTY.MajorStep = 5;
                LinearAxisAirTY.Minimum = min - 2;
                LinearAxisAirTY.Maximum = max + 2;
            }
        }
    }
}
