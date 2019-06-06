using Newtonsoft.Json;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;

namespace C_AWS_Monitor
{
    public partial class TrayWindow : Window
    {
        private string CurrentPage = "null";
        private DataPage _DataPage;

        private bool IsOpen = false;
        private bool IsSettingsOpen = false;
        private bool IsLoading = false;

        private ChartViewModel ChartModel = new ChartViewModel();
        DateTime DataTime;
        DateTime LastUpdated;

        public TrayWindow()
        {
            InitializeComponent();

            if (!Properties.Settings.Default.IsFirstRun)
            {
                _DataPage = new DataPage();
            }
        }

        public void Open()
        {
            if (!IsOpen)
            {
                // Set position of window to bottom right corner of screen
                Rect workingArea = SystemParameters.WorkArea;
                this.Left = workingArea.Right - this.Width - 20;
                this.Top = workingArea.Bottom - this.Height - 20;

                if (Properties.Settings.Default.IsFirstRun && CurrentPage != "Settings")
                {
                    Content.Content = new SettingsPage(true, SettingsDismiss);
                }
                else { LoadData(DataTime == null ? false : true); }

                this.Show();
                IsOpen = true;
                this.Activate();
            }
        }
        public new void Close()
        {
            this.Hide();
            IsOpen = false;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData(false);
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsOpen = true;
            SettingsOverlay.Visibility = Visibility.Visible;

            if (!Properties.Settings.Default.IsFirstRun)
            { DataEndpoint.Text = Properties.Settings.Default.DataEndpoint; }
            DataEndpoint.Focus();
        }

        

        

        private void More_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.DataEndpoint);
        }

        private void SettingsDismiss(string data)
        {
            if (data == "save")
            {

            }
        }
    }
}
