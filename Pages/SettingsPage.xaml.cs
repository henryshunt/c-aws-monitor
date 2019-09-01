using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using C_AWSMonitor.Routines;
using static C_AWSMonitor.Routines.JSON;
using System.Threading;

namespace C_AWSMonitor
{
    public partial class SettingsPage : Page
    {
        private bool IsSetupMode = false;

        public event EventHandler SettingsCheckStarted;
        public event EventHandler SettingsCheckCompleted;
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
            new Thread(delegate ()
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    SettingsCheckStarted?.Invoke(this, new EventArgs());
                    TextBoxEndpoint.IsEnabled = false;
                    ButtonCancel.IsEnabled = false;
                });

                try
                {
                    // Request AWS info to check entered URL is valid
                    string awsInfoUrl = null;
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        awsInfoUrl = Path
                            .Combine(TextBoxEndpoint.Text + "/", "data/aws-info.php");
                    });

                    string awsInfoData = Helpers.RequestURL(awsInfoUrl);
                    if (awsInfoData == "1") throw new Exception("Server-side error");

                    AWSInfo awsInfoJson = JsonConvert.DeserializeObject<AWSInfo>(
                        awsInfoData, new JsonSerializerSettings
                        { NullValueHandling = NullValueHandling.Ignore });

                    Application.Current.Dispatcher.Invoke(delegate
                    { Properties.Settings.Default.DataEndpoint = TextBoxEndpoint.Text; });
                    Properties.Settings.Default.AWSTimeZone = awsInfoJson.TimeZone;
                    Properties.Settings.Default.IsFirstRun = false;
                    Properties.Settings.Default.Save();

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        SettingsCheckCompleted?.Invoke(this, new EventArgs());
                        SettingsDismissed?.Invoke(this,
                            new SettingsDismissedEventArgs(DismissType.Save));
                    });
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        TextBoxEndpoint.IsEnabled = true;
                        TextBoxEndpoint.Focus();
                        TextBoxEndpoint.SelectAll();
                        if (!IsSetupMode)
                            ButtonCancel.IsEnabled = true;

                        SettingsCheckCompleted?.Invoke(this, new EventArgs());
                    });
                }
            }).Start();
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            SettingsDismissed?.Invoke(this,
                new SettingsDismissedEventArgs(DismissType.Cancel));
        }
    }
}
