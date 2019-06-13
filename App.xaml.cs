using C_AWSMonitor.Windows;
using System;
using System.Drawing;
using System.Windows;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace C_AWSMonitor
{
    public partial class App : Application
    {
        private NotifyIcon TrayIcon = new NotifyIcon();
        private TrayWindow TrayWindow = new TrayWindow();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TrayIcon.Text = "C-AWS Station Monitor";
            TrayIcon.Click += TrayIcon_Click;

            TrayWindow.Closed += TrayWindow_Closed;

            TrayIcon.Icon = new Icon("Resources/Icon.ico");
            TrayIcon.Visible = true;
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            TrayWindow.Open();
        }

        private void TrayWindow_Closed(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.Current.Shutdown();
        }
    }
}
