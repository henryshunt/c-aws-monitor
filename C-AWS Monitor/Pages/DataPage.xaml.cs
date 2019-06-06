using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C_AWS_Monitor
{
    /// <summary>
    /// Interaction logic for DataPage.xaml
    /// </summary>
    public partial class DataPage : Page
    {
        public DataPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AirTChart.DataContext = ChartModel;
        }

        private void LoadData(bool checkUpdated)
        {
            if (IsLoading) return;
            if (IsSettingsOpen) return;
            if (checkUpdated)
            {
                TimeSpan difference = DateTime.UtcNow - LastUpdated;
                if (difference.TotalMinutes < 1) return;
            }

            try
            {
                Thread worker = new Thread(delegate ()
                {
                    // Show the loading spinner
                    Application.Current.Dispatcher.Invoke(delegate
                    { Spinner.Visibility = Visibility.Visible; });

                    // Get temporary current time
                    DateTime _LastUpdated = DateTime.UtcNow;
                    string utc = _LastUpdated.ToString("yyyy-MM-dd'T'HH-mm-00");

                    // Download and deserialise report for current time
                    string recordUrl = Properties.Settings.Default.DataEndpoint + "/data/now.php?time=" + utc;
                    string recordData = new WebClient().DownloadString(recordUrl);
                    ReportJSON recordJson = JsonConvert.DeserializeObject<ReportJSON>(recordData,
                        new JsonSerializerSettings
                        { NullValueHandling = NullValueHandling.Ignore });

                    // Download and deserialise environment report for current time
                    string envUrl = Properties.Settings.Default.DataEndpoint + "/data/about.php?time=" + utc;
                    string envData = new WebClient().DownloadString(envUrl);
                    EnvReportJSON envJson = JsonConvert.DeserializeObject<EnvReportJSON>(envData,
                        new JsonSerializerSettings
                        { NullValueHandling = NullValueHandling.Ignore });

                    // Download and deserialise chart data for current day
                    string graphUrl = Properties.Settings.Default.DataEndpoint + "/data/graph-day.php?time=" + utc + "&fields=AirT";
                    string graphData = new WebClient().DownloadString(graphUrl).Replace("[[", "[").Replace("]]", "]");
                    List<PointJSON> graphJson = JsonConvert.DeserializeObject<List<PointJSON>>(graphData,
                        new JsonSerializerSettings
                        { NullValueHandling = NullValueHandling.Ignore });

                    LastUpdated = _LastUpdated;
                    DataTime = DateTime.Parse(recordJson.Time);

                    // Display the downloaded data in the interface
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        RecordTime.Content = "Data on " + DateTime.Parse(
                            recordJson.Time).ToString("dd/MM/yyyy 'at' HH:mm");
                        AirTValue.Content = recordJson.AirT.ToString() + "°C";

                        // Add points to chart
                        ChartModel.ClearPoints();
                        foreach (var item in graphJson)
                        {
                            ChartModel.AddPoint(Convert.ToInt32(item.x), item.y);
                        }

                        RelHValue.Content = recordJson.RelH.ToString() + "%";
                        WSpdValue.Content = recordJson.WSpd.ToString() + " mph";
                        WDirValue.Content = recordJson.WDir.ToString() + "°";
                        WGstValue.Content = recordJson.WGst.ToString() + " mph";
                        SunD_PHrValue.Content = (recordJson.SunD_PHr / 60).ToString() + " min";
                        Rain_PHrValue.Content = recordJson.Rain_PHr.ToString() + " mm";
                        MSLPValue.Content = recordJson.MSLP.ToString() + " hPa";
                        ST10Value.Content = recordJson.ST10.ToString() + "°C";
                        ST30Value.Content = recordJson.ST30.ToString() + "°C";
                        ST00Value.Content = recordJson.ST00.ToString() + "°C";
                        CPUTValue.Content = envJson.CPUT.ToString() + "°C";

                        // Calculate current day boundaries for the chart
                        var bounds = DayBounds(_LastUpdated);
                        GraphXAxis.Minimum = DateTimeAxis.ToDouble(bounds.Item1);
                        GraphXAxis.Maximum = DateTimeAxis.ToDouble(bounds.Item2);

                        SetYAxisSettings();
                        AirTChart.InvalidatePlot();

                        // Hide the loading spinner
                        Spinner.Visibility = Visibility.Hidden;
                    });
                });

                worker.IsBackground = true;
                worker.Start();
            }
            catch
            {
                Spinner.Visibility = Visibility.Hidden;
            }
        }

        private Tuple<DateTime, DateTime> DayBounds(DateTime utc)
        {
            DateTime start = new DateTime(utc.Year, utc.Month, utc.Day);
            DateTime end = start.AddDays(1);

            return new Tuple<DateTime, DateTime>(start, end);
        }
        private void SetYAxisSettings()
        {
            double min = ChartModel.GetMinimum();
            double max = ChartModel.GetMaximum();
            double range = max - min;

            if (range <= 2)
            {
                GraphYAxis.MajorStep = 0.5;
                GraphYAxis.Minimum = min - 0.5;
                GraphYAxis.Maximum = max + 0.5;
            }
            else if (range > 2 && range <= 5)
            {
                GraphYAxis.MajorStep = 1;
                GraphYAxis.Minimum = min - 0.5;
                GraphYAxis.Maximum = max + 0.5;
            }
            else if (range > 5 && range <= 10)
            {
                GraphYAxis.MajorStep = 2;
                GraphYAxis.Minimum = min - 2;
                GraphYAxis.Maximum = max + 2;
            }
            else if (range > 10 && range <= 25)
            {
                GraphYAxis.MajorStep = 5;
                GraphYAxis.Minimum = min - 2;
                GraphYAxis.Maximum = max + 2;
            }
        }

        private class ReportJSON
        {
            public string Time { get; set; }
            public double AirT { get; set; }
            public double ExpT { get; set; }
            public double RelH { get; set; }
            public double DewP { get; set; }
            public double WSpd { get; set; }
            public int WDir { get; set; }
            public double WGst { get; set; }
            public int SunD { get; set; }
            public int SunD_PHr { get; set; }
            public double Rain { get; set; }
            public double Rain_PHr { get; set; }
            public double StaP { get; set; }
            public double MSLP { get; set; }
            public double StaP_PTH { get; set; }
            public double ST10 { get; set; }
            public double ST30 { get; set; }
            public double ST00 { get; set; }
        }
        private class EnvReportJSON
        {
            public string Time { get; set; }
            public double EncT { get; set; }
            public double CPUT { get; set; }
        }
        private class PointJSON
        {
            public string x { get; set; }
            public double y { get; set; }
        }
    }
}
