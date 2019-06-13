using C_AWSMonitor.Routines;
using NodaTime;
using System;
using System.Diagnostics;
using System.Windows;

namespace C_AWSMonitor.Windows
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
            SprocketControlSpinner.Visibility = Visibility.Visible;
            ButtonRefresh.IsEnabled = false;
            ButtonSettings.IsEnabled = false;
            LabelPageTitle.Content = "Refreshing Data...";
        }
        private void _DataPage_DataDownloadCompleted(object sender, EventArgs e)
        {
            SprocketControlSpinner.Visibility = Visibility.Collapsed;
            ButtonRefresh.IsEnabled = true;
            ButtonSettings.IsEnabled = true;

            if (_DataPage.DataTime != DateTime.MinValue)
            {
                DateTimeZone dtz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
                    Properties.Settings.Default.AWSTimeZone);
                Instant utc = Instant.FromDateTimeUtc(_DataPage.DataTime);

                LabelPageTitle.Content = "Data on "
                    + utc.InZone(dtz).ToString("dd/MM/yyyy 'at' HH:mm", null);
            }
        }

        public void Open()
        {
            if (!IsOpen)
            {
                // Set position of window to bottom right corner of screen
                Rect workingArea = SystemParameters.WorkArea;
                Left = workingArea.Right - Width - 20;
                Top = workingArea.Bottom - Height - 20;

                if (Properties.Settings.Default.IsFirstRun)
                {
                    SwitchPage(MonitorPage.Settings);
                    LabelPageTitle.Content = "Setup C-AWS Monitor";
                }
                else
                {
                    SwitchPage(MonitorPage.Data);
                    _DataPage.LoadData(false);
                }

                Show();
                IsOpen = true;
                Activate();
            }
        }
        protected override void OnDeactivated(EventArgs e)
        {
            Hide();
            IsOpen = false;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage == MonitorPage.Data)
                _DataPage.LoadData(true);
        }
        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage != MonitorPage.Settings)
                SwitchPage(MonitorPage.Settings);
        }
        private void ButtonMore_Click(object sender, RoutedEventArgs e)
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
                        FramePageDisplay.Content = _DataPage;
                        ButtonRefresh.IsEnabled = true;
                        ButtonSettings.IsEnabled = true;
                        break;
                    }
                case MonitorPage.Settings:
                    {
                        SettingsPage settingsPage = new SettingsPage();
                        settingsPage.ExitButtonClicked += SettingsPage_ExitButtonClicked;
                        settingsPage.SettingsDismissed += SettingsPage_SettingsDismissed;

                        FramePageDisplay.Content = settingsPage;
                        ButtonRefresh.IsEnabled = false;
                        ButtonSettings.IsEnabled = false;
                        break;
                    }
                default: break;
            }

            if (Properties.Settings.Default.DataEndpoint == null)
                ButtonMore.IsEnabled = false;
            else ButtonMore.IsEnabled = true;
        }
        private void SettingsPage_ExitButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
        private void SettingsPage_SettingsDismissed(object sender, SettingsDismissedEventArgs e)
        {
            SwitchPage(MonitorPage.Data);
            _DataPage.LoadData(e.Dismissal == DismissType.Save ? true : false);
        }
    }

    public enum MonitorPage { None, Settings, Data };
}
