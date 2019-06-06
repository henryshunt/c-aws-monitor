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

            // Set the tray icon from a PNG file
            var bitmap = new Bitmap("Resources/icon.png");
            TrayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
            TrayIcon.Visible = true;
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            TrayWindow.Open();
        }
    }
}
