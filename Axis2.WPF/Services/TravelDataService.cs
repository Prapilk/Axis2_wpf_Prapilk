using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axis2.WPF.Services
{
    public class TravelDataService
    {
        public List<TravelLocation> ParseTravelScript(string filePath)
        {
            var locations = new List<TravelLocation>();
            if (!File.Exists(filePath))
            {
                return locations;
            }

            var lines = File.ReadAllLines(filePath);
            string currentCategory = string.Empty;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//") || string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Check if it's a [LOCATIONS] section
                    if (trimmedLine.Equals("[LOCATIONS]", System.StringComparison.OrdinalIgnoreCase))
                    {
                        currentCategory = "Locations"; // Default category for locations
                    }
                    else
                    {
                        currentCategory = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    }
                }
                else if (!string.IsNullOrEmpty(currentCategory))
                {
                    var parts = trimmedLine.Split(',');
                    if (parts.Length >= 4)
                    {
                        var location = new TravelLocation
                        {
                            Name = parts[0].Trim(),
                            Category = currentCategory,
                            Map = int.TryParse(parts[1].Trim(), out var map) ? map : 0,
                            X = int.TryParse(parts[2].Trim(), out var x) ? x : 0,
                            Y = int.TryParse(parts[3].Trim(), out var y) ? y : 0,
                            Z = parts.Length > 4 && int.TryParse(parts[4].Trim(), out var z) ? z : 0
                        };
                        locations.Add(location);
                    }
                }
            }

            return locations;
        }
    }
}