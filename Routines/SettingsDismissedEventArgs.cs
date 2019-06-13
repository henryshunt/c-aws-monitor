using System;

namespace C_AWSMonitor.Routines
{
    public class SettingsDismissedEventArgs : EventArgs
    {
        public DismissType Dismissal { get; private set; }

        public SettingsDismissedEventArgs(DismissType dismissal)
        {
            Dismissal = dismissal;
        }
    }

    public enum DismissType { Save, Cancel };
}
