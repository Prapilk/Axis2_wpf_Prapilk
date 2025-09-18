using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Axis2.WPF.Services
{
    public class MusicService
    {
        public List<MusicTrack> LoadMusicTracks(string musicDirectory)
        {
            List<MusicTrack> resultTracks = new List<MusicTrack>();
            
            // Map to store ID -> Name/FileName from config.txt
            Dictionary<int, string> configIdToNameMap = new Dictionary<int, string>();
            // Map to store FileName (without extension) -> Full Path from actual MP3 files
            Dictionary<string, string> mp3FileNameToPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(musicDirectory))
            {
                Logger.Log($"WARNING: Music directory not found: {musicDirectory}");
                return resultTracks;
            }

            // Populate mp3FileNameToPathMap
            var mp3Files = Directory.GetFiles(musicDirectory, "*.mp3", SearchOption.TopDirectoryOnly);
            foreach (string mp3File in mp3Files)
            {
                mp3FileNameToPathMap[Path.GetFileNameWithoutExtension(mp3File)] = mp3File;
            }

            // Read config.txt
            string configFile = Path.Combine(musicDirectory, "config.txt");

            if (File.Exists(configFile))
            {
                Regex configLineRegex = new Regex(@"^(\d+)\s+(.*)$"); // ID Name/FileName

                foreach (string line in File.ReadLines(configFile))
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//")) continue; // Skip empty lines and comments

                    Match match = configLineRegex.Match(trimmedLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        string nameOrFileName = match.Groups[2].Value.Trim();
                        configIdToNameMap[id] = nameOrFileName;
                    }
                    else
                    {
                        Logger.Log($"WARNING: Could not parse config.txt line: {trimmedLine}");
                    }
                }
            }
            else
            {
                Logger.Log($"WARNING: config.txt not found: {configFile}");
            }

            // Now, link config entries with actual MP3 files
            foreach (var entry in configIdToNameMap.OrderBy(e => e.Key))
            {
                int id = entry.Key;
                string nameOrFileName = entry.Value;
                string trackName = nameOrFileName; // Default to the name from config.txt
                string filePath = string.Empty;

                // Check if nameOrFileName is an actual MP3 filename (e.g., "57 ConversationWithGwenno.mp3")
                if (nameOrFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(nameOrFileName);
                    if (mp3FileNameToPathMap.TryGetValue(fileNameWithoutExt, out string foundPath))
                    {
                        filePath = foundPath;
                        trackName = fileNameWithoutExt; // Use filename as display name
                    }
                }
                else // It's a descriptive name (e.g., "turfin,loop")
                {
                    // Try to find an MP3 file whose name matches this descriptive name
                    if (mp3FileNameToPathMap.TryGetValue(nameOrFileName, out string foundPath))
                    {
                        filePath = foundPath;
                    }
                    else if (mp3FileNameToPathMap.TryGetValue(nameOrFileName.Split(',')[0].Trim(), out foundPath)) // Handle "name,loop"
                    {
                        filePath = foundPath;
                        trackName = nameOrFileName.Split(',')[0].Trim();
                    }
                }

                resultTracks.Add(new MusicTrack { ID = id, Name = trackName, FilePath = filePath });
            }

            // Add any MP3 files that were not listed in config.txt
            foreach (var mp3Entry in mp3FileNameToPathMap)
            {
                // Check if this MP3 file has already been added from config.txt
                // This is tricky because we don't have a direct ID for these.
                // For now, let's just add them if their filename doesn't match any existing track name.
                if (!resultTracks.Any(t => t.FilePath.Equals(mp3Entry.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    // Assign a high ID to avoid conflicts with config.txt IDs
                    // Or assign a negative ID
                    resultTracks.Add(new MusicTrack { ID = -1, Name = mp3Entry.Key, FilePath = mp3Entry.Value });
                }
            }

            return resultTracks.OrderBy(t => t.ID).ToList();
        }
    }
}