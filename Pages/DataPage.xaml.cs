using Newtonsoft.Json;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using C_AWSMonitor.Routines;
using static C_AWSMonitor.Routines.Helpers;
using static C_AWSMonitor.Routines.JSON;

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

            new Thread(delegate ()
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    { DataDownloadStarted?.Invoke(this, new EventArgs()); });

                    DateTime dataTime = DateTime.UtcNow;
                    string utc = dataTime.ToString("yyyy-MM-dd'T'HH-mm-00");

                    // Download and deserialise report for current time
                    string reportUrl = Path.Combine(Properties.Settings.Default
                        .DataEndpoint + "/", "data/now.php?time=" + utc);

                    string reportData = RequestURL(reportUrl);
                    if (reportData == "1") throw new Exception("Server-side error");
                    Report reportJson = JsonConvert.DeserializeObject<Report>(reportData);

                    // Recalibrate time to returned record time
                    dataTime = DateTime.Parse(reportJson.Time);
                    dataTime = DateTime.SpecifyKind(dataTime, DateTimeKind.Utc);
                    utc = dataTime.ToString("yyyy-MM-dd'T'HH-mm-00");

                    // Download and deserialise chart data for current day
                    string graphUrl = Path.Combine(Properties.Settings.Default.DataEndpoint
                        + "/", "data/graph-day.php?time=" + utc + "&fields=AirT");
                    string graphData = RequestURL(graphUrl);
                    if (graphData == "1") throw new Exception("Server-side error");

                    graphData = graphData.Replace("[[", "[").Replace("]]", "]");
                    List<ChartPoint> graphJson = JsonConvert.DeserializeObject<List<
                        ChartPoint>>(graphData);

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

                        foreach (var item in graphJson)
                        {
                            if (item.Y != null)
                                ChartModel.AddPoint(UTCToLocal(item.X), (double)item.Y);
                        }

                        if (reportJson.RelH != null)
                            LabelRelH.Content = ((double)reportJson.RelH).ToString("0.0") + "%";
                        else LabelRelH.Content = "No Data";

                        if (reportJson.DewP != null)
                            LabelDewP.Content = ((double)reportJson.DewP).ToString("0.0") + "°C";
                        else LabelDewP.Content = "No Data";

                        if (reportJson.WSpd != null)
                            LabelWSpd.Content = ((double)reportJson.WSpd).ToString("0.0") + " mph";
                        else LabelWSpd.Content = "No Data";

                        if (reportJson.WDir != null)
                        {
                            LabelWDir.Content = reportJson.WDir.ToString()
                                + "° (" + DegreesToCompass((int)reportJson.WDir) + ")";
                        }
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

                        // Calculate chart boundaries
                        var bounds = GetDayBounds(UTCToLocal(dataTime));
                        DateTimeAxisAirTX.Minimum = DateTimeAxis.ToDouble(bounds.Item1);
                        DateTimeAxisAirTX.Maximum = DateTimeAxis.ToDouble(bounds.Item2);
                        SetYAxisSettings();
                        PlotAirT.InvalidatePlot();

                        DataDownloadCompleted?.Invoke(this, new EventArgs());
                    });
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    { DataDownloadCompleted?.Invoke(this, new EventArgs()); });
                }
            }).Start();
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
