using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using C_AWSMonitor.Routines;
using static C_AWSMonitor.Routines.JSON;

namespace C_AWSMonitor
{
    public partial class SettingsPage : Page
    {
        private bool IsSetupMode = false;

        public event EventHandler ExitButtonClicked;
        public event EventHandler<SettingsDismissedEventArgs> SettingsDismissed;

        public SettingsPage()
        {
            if (Properties.Settings.Default.IsFirstRun)
                IsSetupMode = true;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetupMode)
            {
                ButtonCancel.IsEnabled = true;
                TextBoxEndpoint.Text = Properties.Settings.Default.DataEndpoint;
            }

            TextBoxEndpoint.Focus();
        }

        private void TextBoxEndpoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxEndpoint.Text.Length == 0)
                ButtonSave.IsEnabled = false;
            else ButtonSave.IsEnabled = true;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ExitButtonClicked?.Invoke(this, new EventArgs());
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Request AWS info to check entered URL is valid
                string awsInfoUrl = Path.Combine(
                    TextBoxEndpoint.Text + "/", "data/aws-info.php");

                string awsInfoData = new TimedWebClient(5000).DownloadString(awsInfoUrl);
                if (awsInfoData == "null") throw new Exception();

                AWSInfo awsInfoJson = JsonConvert.DeserializeObject<AWSInfo>(
                    awsInfoData, new JsonSerializerSettings
                    { NullValueHandling = NullValueHandling.Ignore });

                Properties.Settings.Default.DataEndpoint = TextBoxEndpoint.Text;
                Properties.Settings.Default.AWSTimeZone = awsInfoJson.TimeZone;
                Properties.Settings.Default.IsFirstRun = false;
                Properties.Settings.Default.Save();

                SettingsDismissed?.Invoke(this,
                    new SettingsDismissedEventArgs(DismissType.Save));
            }
            catch
            {
                TextBoxEndpoint.Focus();
                TextBoxEndpoint.Clear();
            }
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            SettingsDismissed?.Invoke(this,
                new SettingsDismissedEventArgs(DismissType.Cancel));
        }
    }
}
