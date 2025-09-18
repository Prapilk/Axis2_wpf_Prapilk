using System;

namespace Axis2.WPF.Services
{
    public interface ISettingsService
    {
        Models.AllSettings LoadSettings();
        event EventHandler<Models.SettingsChangedEventArgs> SettingsChanged;
    }
}
