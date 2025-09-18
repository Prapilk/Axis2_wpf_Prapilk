using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Axis2.WPF.Models;

namespace Axis2.WPF.Services
{
    public class ProfileService
    {
        private readonly string _profilesFilePath = "profiles.json";

        public ObservableCollection<Profile> LoadProfiles()
        {
            if (File.Exists(_profilesFilePath))
            {
                string jsonString = File.ReadAllText(_profilesFilePath);
                var profiles = JsonSerializer.Deserialize<ObservableCollection<Profile>>(jsonString);
                return profiles ?? new ObservableCollection<Profile>();
            }
            return new ObservableCollection<Profile>();
        }

        public void SaveProfiles(ObservableCollection<Profile> profiles)
        {
            string jsonString = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_profilesFilePath, jsonString);
        }
    }
}