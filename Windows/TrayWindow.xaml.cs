﻿using C_AWSMonitor.Routines;
using System;
using System.Diagnostics;
using System.Windows;
using static C_AWSMonitor.Routines.Helpers;

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
            _DataPage.DataDownloadStarted += DataPage_DataDownloadStarted;
            _DataPage.DataDownloadCompleted += DataPage_DataDownloadCompleted;
        }
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                OnDeactivated(new EventArgs());
        }

        private void DataPage_DataDownloadStarted(object sender, EventArgs e)
        {
            SprocketControlA.Visibility = Visibility.Visible;
            ButtonRefresh.IsEnabled = false;
            ButtonSettings.IsEnabled = false;
        }
        private void DataPage_DataDownloadCompleted(object sender, EventArgs e)
        {
            SprocketControlA.Visibility = Visibility.Collapsed;
            ButtonRefresh.IsEnabled = true;
            ButtonSettings.IsEnabled = true;

            if (_DataPage.DataTime != DateTime.MinValue)
            {
                LabelPageTitle.Content = "Data on " + UTCToLocal(
                    _DataPage.DataTime).ToString("dd/MM/yyyy 'at' HH:mm", null);
            }
            else LabelPageTitle.Content = "No Data";
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
                    ButtonMore.IsEnabled = false;
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

                        if (_DataPage.DataTime != DateTime.MinValue)
                        {
                            LabelPageTitle.Content = "Data on " + UTCToLocal(
                                _DataPage.DataTime).ToString("dd/MM/yyyy 'at' HH:mm", null);
                        }
                        else LabelPageTitle.Content = "No Data";
                        break;
                    }
                case MonitorPage.Settings:
                    {
                        SettingsPage settingsPage = new SettingsPage();
                        settingsPage.SettingsCheckStarted += SettingsPage_SettingsCheckStarted;
                        settingsPage.SettingsCheckCompleted += SettingsPage_SettingsCheckCompleted;
                        settingsPage.ExitButtonClicked += SettingsPage_ExitButtonClicked;
                        settingsPage.SettingsDismissed += SettingsPage_SettingsDismissed;

                        FramePageDisplay.Content = settingsPage;
                        ButtonRefresh.IsEnabled = false;
                        ButtonSettings.IsEnabled = false;
                        LabelPageTitle.Content = "Settings";
                        break;
                    }
                default: break;
            }

            if (Properties.Settings.Default.DataEndpoint == null)
                ButtonMore.IsEnabled = false;
            else ButtonMore.IsEnabled = true;
        }
        private void SettingsPage_SettingsCheckStarted(object sender, EventArgs e)
        {
            SprocketControlA.Visibility = Visibility.Visible;
        }
        private void SettingsPage_SettingsCheckCompleted(object sender, EventArgs e)
        {
            SprocketControlA.Visibility = Visibility.Collapsed;
        }
        private void SettingsPage_ExitButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
        private void SettingsPage_SettingsDismissed(object sender, SettingsDismissedEventArgs e)
        {
            SwitchPage(MonitorPage.Data);
            if (e.Dismissal == DismissType.Save) ButtonMore.IsEnabled = true;
            _DataPage.LoadData(e.Dismissal == DismissType.Save ? true : false);
        }
    }

    public enum MonitorPage { None, Settings, Data };
}
