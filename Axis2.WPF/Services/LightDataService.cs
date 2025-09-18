using Axis2.WPF.Models;
using Axis2.WPF.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace Axis2.WPF.Services
{
    public class LightDataService
    {
        private readonly AllSettings _allSettings;
        private readonly IUoArtService _uoArtService;

        public Dictionary<ushort, LightColor> LightColors { get; private set; } = new Dictionary<ushort, LightColor>();
        public List<DrawConfigEntry> DrawConfigs { get; private set; } = new List<DrawConfigEntry>();
        public Dictionary<uint, ItemTileDataItem> TileDataItems { get; private set; } = new Dictionary<uint, ItemTileDataItem>();
        public List<LightMulItem> LightMulItems { get; private set; } = new List<LightMulItem>();

        public LightDataService(AllSettings allSettings, IUoArtService uoArtService)
        {
            _allSettings = allSettings;
            _uoArtService = uoArtService;
            LoadLightData();
        }

        private void LoadLightData()
        {
            string orionDataPath = Path.Combine(_allSettings.FilePathsSettings.DefaultMulPath, "OrionData");
            string lightColorsFilePath = Path.Combine(orionDataPath, "light_colors.txt");
            string drawConfigFilePath = Path.Combine(orionDataPath, "draw_config.txt");

            //Logger.Log($"[LightDataService] Loading light data from: {orionDataPath}");
            //Logger.Log($"[LightDataService] light_colors.txt path: {lightColorsFilePath}");
            //Logger.Log($"[LightDataService] draw_config.txt path: {drawConfigFilePath}");

            LoadLightColors(lightColorsFilePath);
            LoadDrawConfigs(drawConfigFilePath);
            LoadTileData();
            LoadLightMul();
        }

        private void LoadLightColors(string filePath)
        {
            LightColors.Clear();
            if (!File.Exists(filePath))
            {
               //Logger.Log($"Error: light_colors.txt not found at {filePath}");
                return;
            }

            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#")) continue;

                    string[] lineAndComment = trimmedLine.Split(new[] { "//" }, StringSplitOptions.None);
                    string dataPart = lineAndComment[0];
                    string comment = lineAndComment.Length > 1 ? lineAndComment[1].Trim() : string.Empty;

                    string[] parts = dataPart.Split(';');
                    if (parts.Length >= 1)
                    {
                        string idPart = parts[0].Trim();
                        string[] idRangeParts = idPart.Split('-');

                        ushort startId = Convert.ToUInt16(idRangeParts[0].Trim().Replace("0x", ""), 16);
                        ushort endId = startId;
                        if (idRangeParts.Length > 1)
                        {
                            endId = Convert.ToUInt16(idRangeParts[1].Trim().Replace("0x", ""), 16);
                        }

                        ushort drawConfigId = startId; // Default to the first ID if no color is specified
                        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            drawConfigId = Convert.ToUInt16(parts[1].Trim().Replace("0x", ""), 16);
                        }

                        for (ushort id = startId; id <= endId; id++)
                        {
                            if (!LightColors.ContainsKey(id))
                            {
                                var colorValue = _uoArtService.GetColorFromHue(drawConfigId, 16);
                                LightColors.Add(id, new LightColor { Id = id, DrawConfigId = drawConfigId, ColorValue = colorValue, Comment = comment });
                            }
                        }
                    }
                }
                //Logger.Log($"Loaded {LightColors.Count} light colors from {filePath}");
            }
            catch (Exception ex)
            {
                //Logger.Log($"Error loading light_colors.txt: {ex.Message}");
            }
        }

                private void LoadDrawConfigs(string filePath)
        {
            DrawConfigs.Clear();
            if (!File.Exists(filePath))
            {
                //Logger.Log($"Error: draw_config.txt not found at {filePath}");
                return;
            }

            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//")) continue;

                    string[] lineAndComment = trimmedLine.Split(new[] { "//" }, StringSplitOptions.None);
                    string[] parts = lineAndComment[0].Split(';');
                    if (parts.Length >= 8)
                    {
                        ushort id = Convert.ToUInt16(parts[0].Trim().Replace("0x", ""), 16);
                        float zoom = float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float dezoom = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float timeZoom = float.Parse(parts[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float rotation = float.Parse(parts[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        float alternance = float.Parse(parts[5].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        int numberOfColors = int.Parse(parts[6].Trim());
                        List<ushort> colorIds = parts[7].Trim().Split(',').Select(s => Convert.ToUInt16(s.Trim().Replace("0x", ""), 16)).ToList();
                        string comment = lineAndComment.Length > 1 ? lineAndComment[1].Trim() : string.Empty;

                        DrawConfigs.Add(new DrawConfigEntry
                        {
                            Id = id,
                            Zoom = zoom,
                            Dezoom = dezoom,
                            TimeZoom = timeZoom,
                            Rotation = rotation,
                            Alternance = alternance,
                            NumberOfColors = numberOfColors,
                            ColorIds = colorIds,
                            Comment = comment
                        });
                    }
                }
               // Logger.Log($"Loaded {DrawConfigs.Count} draw configs from {filePath}");
            }
            catch (Exception ex)
            {
                //Logger.Log($"Error loading draw_config.txt: {ex.Message}");
            }
        }

        private unsafe void LoadTileData()
        {
            string? tiledataMulPath = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "tiledata.mul")?.FilePath;
            if (string.IsNullOrEmpty(tiledataMulPath) || !File.Exists(tiledataMulPath))
            {
                //Logger.Log($"Error: tiledata.mul not found at {tiledataMulPath}");
                return;
            }

            TileDataItems.Clear();

            // TODO: Implement proper detection for UOAHS format. For now, assume new format.
            bool useNewTileDataFormat = true; // Art.IsUOAHS();

            // Calculate land data length to skip it
            // OldLandTileDataMul size: 4 (flags) + 2 (texID) + 20 (name) = 26 bytes
            // NewLandTileDataMul size: 8 (flags) + 2 (texID) + 20 (name) = 30 bytes
            int landTileDataStructSize = useNewTileDataFormat ? 30 : 26;
            long landDataLength = (long)(0x4000 / 32) * (landTileDataStructSize * 32 + 4); // 512 blocks * (32 structs + 1 header)

            using (var fs = new FileStream(tiledataMulPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Seek(landDataLength, SeekOrigin.Begin); // Skip land data

                var buffer = new byte[fs.Length - fs.Position];
                fs.Read(buffer, 0, buffer.Length);

                GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr ptr = gc.AddrOfPinnedObject();
                long currentOffset = 0;
                ushort id = 0;

                try
                {
                    while (currentOffset < buffer.Length)
                    {
                        // Skip header (4 bytes)
                        currentOffset += 4;

                        for (int i = 0; i < 32; i++)
                        {
                            int itemStructSize = useNewTileDataFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);
                            if (currentOffset + itemStructSize > buffer.Length)
                            {
                                break;
                            }

                            if (useNewTileDataFormat)
                            {
                                NewItemTileDataMul mulStruct = (NewItemTileDataMul)Marshal.PtrToStructure((IntPtr)(ptr.ToInt64() + currentOffset), typeof(NewItemTileDataMul));
                                TileDataItems.Add(id, new ItemTileDataItem(mulStruct, id));
                                currentOffset += itemStructSize;
                            }
                            else
                            {
                                OldItemTileDataMul mulStruct = (OldItemTileDataMul)Marshal.PtrToStructure((IntPtr)(ptr.ToInt64() + currentOffset), typeof(OldItemTileDataMul));
                                TileDataItems.Add(id, new ItemTileDataItem(mulStruct, id));
                                currentOffset += itemStructSize;
                            }
                            id++;
                        }
                    }
                }
                finally
                {
                    gc.Free();
                }
            }
            //Logger.Log($"Loaded {TileDataItems.Count} item tile data entries from {tiledataMulPath}");
        }

        private void LoadLightMul()
        {
            string? lightMulPath = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "light.mul")?.FilePath;
            if (string.IsNullOrEmpty(lightMulPath) || !File.Exists(lightMulPath))
            {
                //Logger.Log($"Error: light.mul not found at {lightMulPath}");
                return;
            }

            LightMulItems.Clear();
            using (var reader = new BinaryReader(File.Open(lightMulPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                ushort id = 0;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    if (reader.BaseStream.Position + 26 > reader.BaseStream.Length)
                    {
                        break;
                    }
                    var item = new LightMulItem
                    {
                        Id = id,
                        ColorTable = reader.ReadBytes(22),
                        Unknown = reader.ReadInt32()
                    };
                    LightMulItems.Add(item);
                    id++;
                }
            }
            //Logger.Log($"Loaded {LightMulItems.Count} light definitions from {lightMulPath}");
        }
    }
}
