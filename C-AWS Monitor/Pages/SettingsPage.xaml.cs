using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace C_AWS_Monitor
{
    public partial class SettingsPage : Page
    {
        private bool IsSetupMode = false;
        private Action<bool> DismissCallback;

        public SettingsPage(Action<bool> dismissCallback)
        {
            if (Properties.Settings.Default.IsFirstRun)
                IsSetupMode = true;

            DismissCallback = dismissCallback;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsSetupMode)
                Cancel.IsEnabled = false;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new WebClient().DownloadString(dataEndpoint.Text);
                Properties.Settings.Default.DataEndpoint = dataEndpoint.Text;
                Properties.Settings.Default.IsFirstRun = false;
                Properties.Settings.Default.Save();
                DismissCallback(true);
            }
            catch
            {
                dataEndpoint.Focus();
                dataEndpoint.SelectAll();
            }
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DismissCallback(false);
        }
    }
}
