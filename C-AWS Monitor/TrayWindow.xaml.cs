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
        private bool IsOpen = false;

        private MonitorPage CurrentPage = MonitorPage.None;
        private DataPage _DataPage = new DataPage();

        public TrayWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _DataPage.DataDownloadStarted += _DataPage_DataDownloadStarted;
            _DataPage.DataDownloadCompleted += _DataPage_DataDownloadCompleted;
        }

        private void _DataPage_DataDownloadStarted(object sender, EventArgs e)
        {
            Spinner.Visibility = Visibility.Visible;
        }
        private void _DataPage_DataDownloadCompleted(object sender, EventArgs e)
        {
            Spinner.Visibility = Visibility.Collapsed;

            if (_DataPage.DataTime != null)
            {
                pageTitle.Content = "Data on "
                    + _DataPage.DataTime.ToString("dd/MM/yyyy 'at' HH:mm");
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

                if (Properties.Settings.Default.IsFirstRun)
                    SwitchPage(MonitorPage.Settings);
                else
                {
                    SwitchPage(MonitorPage.Data);
                    _DataPage.LoadData(true);
                }

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

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage == MonitorPage.Data)
                _DataPage.LoadData(false);
        }
        private void settings_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage != MonitorPage.Settings)
                SwitchPage(MonitorPage.Settings);
        }
        private void more_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.DataEndpoint != null)
                Process.Start(Properties.Settings.Default.DataEndpoint);
        }

        private void SwitchPage(MonitorPage targetPage)
        {
            CurrentPage = targetPage;

            switch (targetPage)
            {
                case MonitorPage.Data:
                    {
                        pageDisplay.Content = _DataPage;
                        refresh.IsEnabled = true;
                        settings.IsEnabled = true;

                        if (Properties.Settings.Default.DataEndpoint == null)
                            more.IsEnabled = false;
                        else more.IsEnabled = true;
                        break;
                    }
                case MonitorPage.Settings:
                    {
                        pageDisplay.Content = new SettingsPage(SettingsDismiss);
                        refresh.IsEnabled = false;
                        settings.IsEnabled = false;

                        if (Properties.Settings.Default.DataEndpoint == null)
                            more.IsEnabled = false;
                        else more.IsEnabled = true;
                        break;
                    }
                default: break;
            }
        }

        private void SettingsDismiss(bool didSave)
        {
            SwitchPage(MonitorPage.Data);
            _DataPage.LoadData(didSave ? false : true);
        }

        private enum MonitorPage { None, Settings, Data };
    }
}
