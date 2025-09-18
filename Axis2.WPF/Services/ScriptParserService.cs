using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;
using Point = System.Windows.Point;

namespace Axis2.WPF.Services
{
    public class ScriptParserService
    {
                public List<RegionGroup> ParseMapScript(IEnumerable<string>? scriptPaths)
        {
            if (scriptPaths == null)
            {
                Logger.Log("ERROR: ScriptParserService.ParseMapScript received a null scriptPaths collection.");
                return new List<RegionGroup>();
            }

            var groups = new Dictionary<string, RegionGroup>();
            try
            {
                foreach (var scriptPath in scriptPaths)
                {
                    if (!File.Exists(scriptPath))
                    {
                        Logger.Log($"Script file not found: {scriptPath}");
                        continue;
                    }

                    var lines = File.ReadAllLines(scriptPath);
                    MapRegion? currentRegion = null;
                    AreaDefinition? currentAreaDefinition = null; // Track the current AreaDefinition

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("[AREADEF"))
                        {
                            currentAreaDefinition = new AreaDefinition();
                            currentRegion = currentAreaDefinition;
                        }
                        else if (trimmedLine.StartsWith("[ROOMDEF"))
                        {
                            RoomDefinition room = new RoomDefinition();
                            if (currentAreaDefinition != null)
                            {
                                currentAreaDefinition.Rooms.Add(room); // Add to parent AreaDefinition
                            }
                            currentRegion = room; // Properties will be parsed into this room
                        }
                        else if (currentRegion != null && trimmedLine.Contains('='))
                        {
                            var parts = trimmedLine.Split(new[] { '=' }, 2);
                            var key = parts[0].Trim().ToUpper();
                            var value = parts[1].Trim();

                            switch (key)
                            {
                                case "NAME":
                                    currentRegion.Name = value;
                                    break;
                                case "GROUP":
                                    currentRegion.Group = value;
                                    // Only add to RegionGroup if it's a top-level AreaDefinition or RoomDefinition
                                    if (currentRegion is AreaDefinition || (currentRegion is RoomDefinition && currentAreaDefinition == null))
                                    {
                                        ParseAndAddRegion(currentRegion, groups, trimmedLine);
                                    }
                                    break;
                                case "RECT":
                                    var rectParts = value.Split(',');
                                    if (rectParts.Length >= 4) // Minimum x1,y1,x2,y2
                                    {
                                        try
                                        {
                                            int x1 = int.Parse(rectParts[0]);
                                            int y1 = int.Parse(rectParts[1]);
                                            int x2 = int.Parse(rectParts[2]);
                                            int y2 = int.Parse(rectParts[3]);
                                            int map = (rectParts.Length >= 5 && int.TryParse(rectParts[4], out int parsedMap)) ? parsedMap : 0; // Get map if present, else 0

                                            // Ensure width and height are non-negative
                                            int rectX = System.Math.Min(x1, x2);
                                            int rectY = System.Math.Min(y1, y2);
                                            int rectWidth = System.Math.Abs(x2 - x1);
                                            int rectHeight = System.Math.Abs(y2 - y1);

                                            currentRegion.Rects.Add(new Rect(rectX, rectY, rectWidth, rectHeight));
                                            // Only set Map if P hasn't set it, or if it's the first RECT
                                            if (currentRegion.Map == 0) // Assuming 0 is default/unset map
                                            {
                                                currentRegion.Map = map;
                                            }
                                        }
                                        catch (System.FormatException ex)
                                        {
                                            Logger.Log($"Error parsing RECT coordinates in {scriptPath}, line: '{line}'. Details: {ex.Message}");
                                        }
                                        catch (System.ArgumentException ex)
                                        {
                                            Logger.Log($"Invalid RECT dimensions in {scriptPath}, line: '{line}'. Details: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log($"Invalid RECT format in {scriptPath}, line: '{line}'. Expected at least 4 parts (x1,y1,x2,y2).");
                                    }
                                    break;
                                case "P":
                                    var pParts = value.Split(',');
                                    if (pParts.Length >= 2) // Minimum X,Y
                                    {
                                        try
                                        {
                                            currentRegion.P = new Point(int.Parse(pParts[0]), int.Parse(pParts[1]));
                                            currentRegion.Z = (pParts.Length >= 3 && int.TryParse(pParts[2], out int parsedZ)) ? parsedZ : 0; // Get Z if present, else 0
                                            currentRegion.Map = (pParts.Length >= 4 && int.TryParse(pParts[3], out int parsedMap)) ? parsedMap : 0; // Get Map if present, else 0
                                        }
                                        catch (System.FormatException ex)
                                        {
                                            Logger.Log($"Error parsing P coordinates in {scriptPath}, line: '{line}'. Details: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log($"Invalid P format in {scriptPath}, line: '{line}'. Expected at least 2 parts.");
                                    }
                                    break;
                            }
                        }
                    }
                }

                return groups.Values.ToList();
            }
            catch (System.Exception ex)
            {
                Logger.Log($"CRITICAL ERROR in ScriptParserService.ParseMapScript: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new List<RegionGroup>(); // Return an empty list to prevent NullReferenceException
            }
        }

        public List<SpawnGroup> ParseSpawnScripts(IEnumerable<string>? scriptPaths)
        {
            var spawnGroups = new List<SpawnGroup>();
            if (scriptPaths == null)
            {
                Logger.Log("ERROR: ScriptParserService.ParseSpawnScripts received a null scriptPaths collection.");
                return spawnGroups;
            }

            try
            {
                foreach (var scriptPath in scriptPaths)
                {
                    if (!File.Exists(scriptPath))
                    {
                        Logger.Log($"Script file not found: {scriptPath}");
                        continue;
                    }

                    var lines = File.ReadAllLines(scriptPath);
                    SpawnGroup? currentSpawnGroup = null;

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("[SPAWN "))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(trimmedLine, @"\\[SPAWN\s+([^\\]]+)\\]");
                            if (match.Success)
                            {
                                currentSpawnGroup = new SpawnGroup { Name = match.Groups[1].Value.Trim() };
                                spawnGroups.Add(currentSpawnGroup);
                            }
                            else
                            {
                                Logger.Log($"Warning: Malformed [SPAWN] header in {scriptPath}: {trimmedLine}");
                                currentSpawnGroup = null; // Reset to avoid parsing properties into a bad group
                            }
                        }
                        else if (currentSpawnGroup != null && trimmedLine.Contains('='))
                        {
                            var parts = trimmedLine.Split(new[] { '=' }, 2);
                            var key = parts[0].Trim().ToUpper();
                            var value = parts[1].Trim();

                            switch (key)
                            {
                                case "CATEGORY":
                                    currentSpawnGroup.Category = value;
                                    break;
                                case "SUBSECTION":
                                    currentSpawnGroup.SubSection = value;
                                    break;
                                case "DESCRIPTION":
                                    currentSpawnGroup.Description = value;
                                    break;
                                case "ID":
                                    ParseAndAddNpc(currentSpawnGroup, value);
                                    break;
                                // Add other properties if needed
                            }
                        }
                        else if (currentSpawnGroup != null && !string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                        {
                            if (!trimmedLine.Contains('='))
                            {
                                ParseAndAddNpc(currentSpawnGroup, trimmedLine);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                return new List<SpawnGroup>(); // Return empty list on critical error
            }

            return spawnGroups;
        }

        private void ParseAndAddNpc(SpawnGroup spawnGroup, string npcId)
        {
            if (string.IsNullOrWhiteSpace(npcId)) return;

            var regex = new System.Text.RegularExpressions.Regex(@"^([a-zA-Z_][a-zA-Z0-9_\.]*)(\d*)$");
            var match = regex.Match(npcId);

            if (match.Success)
            {
                string defName = match.Groups[1].Value;
                int amount = 1; // Default amount
                if (match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    if (int.TryParse(match.Groups[2].Value, out int parsedAmount) && parsedAmount > 0)
                    {
                        amount = parsedAmount;
                    }
                }
                
                spawnGroup.SpawnEntries.Add(new SpawnEntry { DefName = defName, Amount = amount });
            }
            else
            {
                // If regex doesn't match, assume it's a DefName with amount 1
                spawnGroup.SpawnEntries.Add(new SpawnEntry { DefName = npcId, Amount = 1 });
            }
        }

        public void SaveMapScript(IEnumerable<RegionGroup> regionGroups, string scriptPath)

        {
            try
            {
                using (StreamWriter writer = new StreamWriter(scriptPath))
                {
                    foreach (var group in regionGroups)
                    {
                        foreach (var area in group.Areas)
                        {
                            writer.WriteLine($"[AREADEF {area.Name}]");
                            writer.WriteLine($"NAME={area.Name}");
                            writer.WriteLine($"GROUP={area.Group}");
                            if (area.P.X != 0 || area.P.Y != 0)
                            {
                                writer.WriteLine($"P={area.P.X},{area.P.Y},{area.Z},{area.Map}");
                            }
                            foreach (var rect in area.Rects)
                            {
                                writer.WriteLine($"RECT={rect.X},{rect.Y},{rect.X + rect.Width},{rect.Y + rect.Height},{area.Map}");
                            }
                            writer.WriteLine(""); // Empty line for separation

                            foreach (var room in area.Rooms)
                            {
                                writer.WriteLine($"[ROOMDEF {room.Name}]");
                                writer.WriteLine($"NAME={room.Name}");
                                writer.WriteLine($"GROUP={room.Group}");
                                if (!string.IsNullOrEmpty(room.DefName))
                                {
                                    writer.WriteLine($"DEFNAME={room.DefName}");
                                }
                                writer.WriteLine($"P={room.P.X},{room.P.Y},{room.Z},{room.Map}");
                                foreach (var rect in room.Rects)
                                {
                                    writer.WriteLine($"RECT={rect.X},{rect.Y},{rect.X + rect.Width},{rect.Y + rect.Height},{room.Map}");
                                }
                                writer.WriteLine(""); // Empty line for separation
                            }
                        }

                        foreach (var room in group.Rooms)
                        {
                            // Handle top-level rooms not associated with an AreaDefinition
                            writer.WriteLine($"[ROOMDEF {room.Name}]");
                            writer.WriteLine($"NAME={room.Name}");
                            writer.WriteLine($"GROUP={room.Group}");
                            if (!string.IsNullOrEmpty(room.DefName))
                            {
                                writer.WriteLine($"DEFNAME={room.DefName}");
                            }
                            writer.WriteLine($"P={room.P.X},{room.P.Y},{room.Z},{room.Map}");
                            foreach (var rect in room.Rects)
                            {
                                writer.WriteLine($"RECT={rect.X},{rect.Y},{rect.X + rect.Width},{rect.Y + rect.Height},{room.Map}");
                            }
                            writer.WriteLine(""); // Empty line for separation
                        }
                    }
                }
                Logger.Log($"Successfully saved map script to: {scriptPath}");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"ERROR: Failed to save map script to {scriptPath}. Details: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private void ParseAndAddRegion(MapRegion region, Dictionary<string, RegionGroup> groups, string defLine)
        {
            if (region.Group != null)
            {
                if (!groups.TryGetValue(region.Group, out var group))
                {
                    group = new RegionGroup { Name = region.Group };
                    groups[region.Group] = group;
                }

                if (region is AreaDefinition area)
                {
                    if (!group.Areas.Any(a => a.Name == area.Name))
                    {
                        group.Areas.Add(area);
                    }
                }
                else if (region is RoomDefinition room)
                {
                    if (!group.Rooms.Any(r => r.Name == room.Name))
                    {
                        group.Rooms.Add(room);
                    }
                }
            }
        }
    }
}