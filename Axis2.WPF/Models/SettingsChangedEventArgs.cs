using System;

namespace Axis2.WPF.Models
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public AllSettings AllSettings { get; }

        public SettingsChangedEventArgs(AllSettings allSettings)
        {
            AllSettings = allSettings;
        }
    }
}