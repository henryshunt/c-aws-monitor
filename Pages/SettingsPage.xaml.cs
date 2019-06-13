using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using C_AWSMonitor.Routines;

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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ExitButtonClicked?.Invoke(this, new EventArgs());
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string awsInfoUrl = Path.Combine(
                    TextBoxEndpoint.Text + "/", "data/aws-info.php");

                string awsInfoData = new WebClient().DownloadString(awsInfoUrl);
                AWSInfoJSON awsInfoJson = JsonConvert.DeserializeObject<AWSInfoJSON>(
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
