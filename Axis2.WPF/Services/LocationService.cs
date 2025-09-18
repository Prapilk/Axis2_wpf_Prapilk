using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Axis2.WPF.Services
{
    public class LocationService
    {
        private const string CUSTOM_LOCATIONS_FILE = "custom_locations.json";

        public List<TravelLocation> LoadCustomLocations()
        {
            if (File.Exists(CUSTOM_LOCATIONS_FILE))
            {
                string jsonString = File.ReadAllText(CUSTOM_LOCATIONS_FILE);
                return JsonSerializer.Deserialize<List<TravelLocation>>(jsonString) ?? new List<TravelLocation>();
            }
            return new List<TravelLocation>();
        }

        public void SaveCustomLocations(List<TravelLocation> locations)
        {
            string jsonString = JsonSerializer.Serialize(locations, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CUSTOM_LOCATIONS_FILE, jsonString);
        }
    }
}