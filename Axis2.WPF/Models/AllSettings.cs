using Axis2.WPF.ViewModels.Settings;

namespace Axis2.WPF.Models
{
    public class AllSettings
    {
        public SettingsGeneralViewModel GeneralSettings { get; set; }
        public SettingsFilePathsViewModel FilePathsSettings { get; set; }
        public SettingsItemTabViewModel ItemTabSettings { get; set; }
        public SettingsTravelTabViewModel TravelTabSettings { get; set; }
        public SettingsSpawnTabViewModel SpawnTabSettings { get; set; }
        public SettingsOverridePathsViewModel OverridePathsSettings { get; set; }

        public AllSettings()
        {
            GeneralSettings = new SettingsGeneralViewModel();
            FilePathsSettings = new SettingsFilePathsViewModel();
            ItemTabSettings = new SettingsItemTabViewModel();
            TravelTabSettings = new SettingsTravelTabViewModel();
            SpawnTabSettings = new SettingsSpawnTabViewModel();
            OverridePathsSettings = new SettingsOverridePathsViewModel();
            OverridePathsSettings.SetFilePathsSettings(FilePathsSettings);
        }
    }
}
