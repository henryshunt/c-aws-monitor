using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public partial class SettingsPage : Page
    {
        private bool IsSetupMode = false;
        private Action<string> DismissCallback = null;

        public SettingsPage(bool isSetupMode, Action<string> dismissCallback)
        {
            IsSetupMode = isSetupMode;
            DismissCallback = dismissCallback;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsSetupMode)
                Cancel.IsEnabled = false;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new WebClient().DownloadString(DataEndpoint.Text);
                Properties.Settings.Default.DataEndpoint = DataEndpoint.Text;
                Properties.Settings.Default.IsFirstRun = false;
                Properties.Settings.Default.Save();

                //IsSettingsOpen = false;
                Cancel.IsEnabled = true;
                //SettingsOverlay.Visibility = Visibility.Hidden;
                //LoadData(true);
                DismissCallback("save");
            }
            catch
            {
                DataEndpoint.Focus();
                DataEndpoint.SelectAll();
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DismissCallback("cancel");
            //IsSettingsOpen = false;
            //SettingsOverlay.Visibility = Visibility.Hidden;
            //LoadData(true);
        }
    }
}
