using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using Axis2.WPF.Models;
using Axis2.WPF.ViewModels.Settings;
using System.Linq;
using System.IO.MemoryMappedFiles;

namespace Axis2.WPF.Services
{
    public class MulMapService : IDisposable
    {
        private const int BLOCK_SIZE = 8; // 8x8 tiles per block
        private System.Windows.Media.Color[] _colorMap; // Palette from radarcol.mul
        private readonly SettingsService _settingsService;
        private MemoryMappedFileService? _mapFileService;

        public MulMapService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _colorMap = new System.Windows.Media.Color[65536]; // Initialize to prevent CS8618
            LoadRadarcol();
        }

        public void Dispose()
        {
            _mapFileService?.Dispose();
        }

        private void LoadRadarcol()
        {
            var settings = _settingsService.LoadSettings();
            var radarcolPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "radarcol.mul")?.FilePath;

           // Logger.Log($"Attempting to load radarcol.mul from: {radarcolPath}");

            if (string.IsNullOrEmpty(radarcolPath) || !File.Exists(radarcolPath))
            {
               // Logger.Log("radarcol.mul not found or path is empty. Initializing color map with black.");
                for (int i = 0; i < 65536; i++)
                {
                    _colorMap[i] = Colors.Black;
                }
                return;
            }

            try
            {
                using (var fileStream = new FileStream(radarcolPath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < 65536; i++)
                    {
                        ushort mulColor = reader.ReadUInt16();
                        _colorMap[i] = ScaleColor(mulColor);
                    }
                }
               // Logger.Log("radarcol.mul loaded successfully.");
                //Logger.Log($"  _colorMap[0]: {_colorMap[0]}");
                //Logger.Log($"  _colorMap[100]: {_colorMap[100]}");
                //Logger.Log($"  _colorMap[65535]: {_colorMap[65535]}");
            }
            catch (Exception ex)
            {
               // Logger.Log($"Error loading radarcol.mul: {ex.Message}");
                for (int i = 0; i < 65536; i++)
                {
                    _colorMap[i] = Colors.Red; // Indicate error with red
                }
            }
        }

        private System.Windows.Media.Color ScaleColor(ushort mulColor)
        {
            byte red = (byte)(((mulColor >> 0) & 0x1F) * 255 / 31);
            byte green = (byte)(((mulColor >> 5) & 0x1F) * 255 / 31);
            byte blue = (byte)(((mulColor >> 10) & 0x1F) * 255 / 31);
            return System.Windows.Media.Color.FromArgb(255, red, green, blue);
        }

        private (int width, int height) GetMapDimensions(int mapIndex)
        {
            switch (mapIndex)
            {
                case 0: return (6144, 4096); // Felucca, some clients use 6144
                case 1: return (7168, 4096); // Trammel, some clients use 6144
                case 2: return (2304, 1600); // Ilshenar
                case 3: return (2560, 2048); // Malas
                case 4: return (1448, 1448); // Tokuno
                case 5: return (1280, 4096); // Ter Mur
                default: return (6144, 4096); // Default fallback
            }
        }

        public WriteableBitmap? RenderMap(int mapIndex, string mapPath, int viewPortWidth, int viewPortHeight, int zoomLevel, int centerX, int centerY, StaticsService? staticsService)
        {
           // Logger.Log($"--- RenderMap START for map index: {mapIndex} ---");
            //Logger.Log($"RenderMap: Path='{mapPath}', Viewport={viewPortWidth}x{viewPortHeight}, Zoom={zoomLevel}, Center=({centerX},{centerY})");

            if (!File.Exists(mapPath) || staticsService == null)
            {
                //Logger.Log($"ERROR: Map file not found ('{mapPath}') or StaticsService is null.");
                return null;
            }

            if (_mapFileService == null || !(_mapFileService.FilePath?.Equals(mapPath, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                _mapFileService?.Dispose();
                _mapFileService = new MemoryMappedFileService(mapPath);
               // Logger.Log("RenderMap: New MemoryMappedFileService created for map.");
            }

            if (!_mapFileService.IsOpen)
            {
               // Logger.Log($"ERROR: Failed to open map file with MemoryMappedFileService: {mapPath}");
                return null;
            }

            var (fullMapWidth, fullMapHeight) = GetMapDimensions(mapIndex);
            //Logger.Log($"RenderMap: Using map dimensions {fullMapWidth}x{fullMapHeight} for map index {mapIndex}.");

            int outputPixelWidth = viewPortWidth;
            int outputPixelHeight = viewPortHeight;

            int iPixelsPerCell = 1;
            int iCellsPerBlock = BLOCK_SIZE;

            if (zoomLevel > 0)
            {
                iPixelsPerCell = (int)Math.Pow(2, zoomLevel);
            }
            else if (zoomLevel < 0)
            {
                iCellsPerBlock = BLOCK_SIZE / ((int)Math.Pow(2, Math.Abs(zoomLevel)));
                if (iCellsPerBlock < 1) iCellsPerBlock = 1;
            }

            double currentScaleFactor = zoomLevel >= 0 ? Math.Pow(2, zoomLevel) : 1.0 / Math.Pow(2, Math.Abs(zoomLevel));

            double viewPortOriginX_map = centerX - (outputPixelWidth / 2.0) / currentScaleFactor;
            double viewPortOriginY_map = centerY - (outputPixelHeight / 2.0) / currentScaleFactor;

            int mapWidthInBlocks = fullMapWidth / BLOCK_SIZE;
            int mapHeightInBlocks = fullMapHeight / BLOCK_SIZE;

            int xStartBlock = (int)(viewPortOriginX_map / BLOCK_SIZE);
            int yStartBlock = (int)(viewPortOriginY_map / BLOCK_SIZE);
            int xEndBlock = (int)((viewPortOriginX_map + outputPixelWidth / currentScaleFactor) / BLOCK_SIZE) + 1;
            int yEndBlock = (int)((viewPortOriginY_map + outputPixelHeight / currentScaleFactor) / BLOCK_SIZE) + 1;

            //Logger.Log($"RenderMap: Calculated block range: X=({xStartBlock} to {xEndBlock}), Y=({yStartBlock} to {yEndBlock})");

            xStartBlock = Math.Max(0, xStartBlock);
            yStartBlock = Math.Max(0, yStartBlock);
            xEndBlock = Math.Min(mapWidthInBlocks, xEndBlock);
            yEndBlock = Math.Min(mapHeightInBlocks, yEndBlock);
            
            //Logger.Log($"RenderMap: Clamped block range: X=({xStartBlock} to {xEndBlock}), Y=({yStartBlock} to {yEndBlock})");

            var bitmap = new WriteableBitmap(outputPixelWidth, outputPixelHeight, 96, 96, PixelFormats.Bgr32, null);
            byte[] pixelData = new byte[outputPixelWidth * outputPixelHeight * 4];

            try
            {
                // Terrain Rendering
                for (int blockX = xStartBlock; blockX < xEndBlock; blockX++)
                {
                    for (int blockY = yStartBlock; blockY < yEndBlock; blockY++)
                    {
                        if (blockX < 0 || blockY < 0 || blockX >= mapWidthInBlocks || blockY >= mapHeightInBlocks) continue;

                        long blockOffset = (long)(blockX * mapHeightInBlocks + blockY) * 196;
                        _mapFileService.ReadInt32(blockOffset);

                        for (int y = 0; y < BLOCK_SIZE; y++)
                        {
                            for (int x = 0; x < BLOCK_SIZE; x++)
                            {
                                long cellOffset = blockOffset + 4 + (y * BLOCK_SIZE + x) * 3;
                                ushort tileID = _mapFileService.ReadUInt16(cellOffset);
                                System.Windows.Media.Color color = GetColorForTileID(tileID);

                                int currentPixelX = (blockX * BLOCK_SIZE) + x;
                                int currentPixelY = (blockY * BLOCK_SIZE) + y;

                                for (int py = 0; py < iPixelsPerCell; py++)
                                {
                                    for (int px = 0; px < iPixelsPerCell; px++)
                                    {
                                        int finalPixelX = (int)((currentPixelX - viewPortOriginX_map) * currentScaleFactor) + px;
                                        int finalPixelY = (int)((currentPixelY - viewPortOriginY_map) * currentScaleFactor) + py;

                                        if (finalPixelX >= 0 && finalPixelX < outputPixelWidth &&
                                            finalPixelY >= 0 && finalPixelY < outputPixelHeight)
                                        {
                                            int pixelIndex = (finalPixelY * outputPixelWidth + finalPixelX) * 4;
                                            pixelData[pixelIndex + 0] = color.B;
                                            pixelData[pixelIndex + 1] = color.G;
                                            pixelData[pixelIndex + 2] = color.R;
                                            pixelData[pixelIndex + 3] = 255;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Statics Rendering
                for (int blockX = xStartBlock; blockX < xEndBlock; blockX++)
                {
                    for (int blockY = yStartBlock; blockY < yEndBlock; blockY++)
                    {
                        if (blockX < 0 || blockY < 0 || blockX >= mapWidthInBlocks || blockY >= mapHeightInBlocks) continue;

                        sbyte[] terrainZ_forBlock = GetMapBlockAltitudes(blockX, blockY, mapHeightInBlocks);
                        var statics = staticsService.GetStaticsForBlock(blockX, blockY, mapHeightInBlocks);

                        foreach (var staticTile in statics)
                        {
                            int tileX = (blockX * BLOCK_SIZE) + staticTile.X;
                            int tileY = (blockY * BLOCK_SIZE) + staticTile.Y;

                            sbyte terrainZ = terrainZ_forBlock[staticTile.Y * BLOCK_SIZE + staticTile.X];

                            if (staticTile.Z >= terrainZ)
                            {
                                int drawX = (int)((tileX - viewPortOriginX_map) * currentScaleFactor);
                                int drawY = (int)((tileY - viewPortOriginY_map) * currentScaleFactor);

                                ushort color16bit = staticsService.GetRadarcolColor(staticTile.TileID + 0x4000);
                                byte r = (byte)(((color16bit >> 10) & 0x1F) << 3);
                                byte g = (byte)(((color16bit >> 5) & 0x1F) << 3);
                                byte b = (byte)((color16bit & 0x1F) << 3);

                                if (drawX >= 0 && drawX + iPixelsPerCell <= outputPixelWidth &&
                                    drawY >= 0 && drawY + iPixelsPerCell <= outputPixelHeight)
                                {
                                    byte[] colorData = { b, g, r, 255 };
                                    for (int py = 0; py < iPixelsPerCell; py++)
                                    {
                                        for (int px = 0; px < iPixelsPerCell; px++)
                                        {
                                            int pixelIndex = ((drawY + py) * outputPixelWidth + (drawX + px)) * 4;
                                            if (pixelIndex >= 0 && pixelIndex + 3 < pixelData.Length)
                                            {
                                                pixelData[pixelIndex + 0] = colorData[0];
                                                pixelData[pixelIndex + 1] = colorData[1];
                                                pixelData[pixelIndex + 2] = colorData[2];
                                                pixelData[pixelIndex + 3] = colorData[3];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // Logger.Log($"FATAL ERROR during map rendering loop: {ex.Message}\n{ex.StackTrace}");
                return null;
            }

            bitmap.WritePixels(new Int32Rect(0, 0, outputPixelWidth, outputPixelHeight), pixelData, outputPixelWidth * 4, 0);
            //Logger.Log($"--- RenderMap END ---");
            return bitmap;
        }

        private System.Windows.Media.Color GetColorForTileID(ushort tileID)
        {
            if (tileID < _colorMap.Length)
            {
                return _colorMap[tileID];
            }
            //Logger.Log($"Invalid TileID: {tileID}. Returning Magenta.");
            return Colors.Magenta; // Fallback color for invalid tile IDs
        }

        public sbyte[] GetMapBlockAltitudes(int blockX, int blockY, int mapHeightInBlocks)
        {
            sbyte[] altitudes = new sbyte[BLOCK_SIZE * BLOCK_SIZE];

            if (_mapFileService == null || !_mapFileService.IsOpen)
            {
               // Logger.Log("Map file service is not available.");
                return altitudes;
            }

            long blockOffset = (long)(blockX * mapHeightInBlocks + blockY) * 196;

            try
            {
                // Correction : utiliser la mme logique d'indexation 2D que dans le rendu terrain
                for (int y = 0; y < BLOCK_SIZE; y++)
                {
                    for (int x = 0; x < BLOCK_SIZE; x++)
                    {
                        long cellOffset = blockOffset + 4 + (y * BLOCK_SIZE + x) * 3 + 2; // +2 pour l'altitude
                        altitudes[y * BLOCK_SIZE + x] = _mapFileService.ReadSByte(cellOffset);
                    }
                }
            }
            catch (Exception ex)
            {
               // Logger.Log($"Error getting map block altitudes: {ex.Message}");
            }

            return altitudes;
        }
    }
}