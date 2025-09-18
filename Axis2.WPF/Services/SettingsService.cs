using System.IO;
using System.Text.Json;
using Axis2.WPF.Models;
using Axis2.WPF.ViewModels.Settings;
using System; // Added for EventHandler

namespace Axis2.WPF.Services
{
    public class SettingsService : ISettingsService // Implements ISettingsService
    {
        private readonly string _settingsFilePath = "settings.json";

        public event EventHandler<Models.SettingsChangedEventArgs> SettingsChanged; // Implements event from ISettingsService

        public AllSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                string jsonString = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AllSettings>(jsonString);
                return settings ?? new AllSettings();
            }
            return new AllSettings(); // Return default settings if file doesn't exist
        }

        public void SaveSettings(AllSettings settings)
        {
            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, jsonString);
            // Raise the event after saving settings
            SettingsChanged?.Invoke(this, new Models.SettingsChangedEventArgs(settings));
        }
    }
}