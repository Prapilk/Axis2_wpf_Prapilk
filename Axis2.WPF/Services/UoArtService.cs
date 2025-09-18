using System;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Axis2.WPF.Models;
using Axis2.WPF.ViewModels.Settings;
using System.Globalization;

namespace Axis2.WPF.Services
{
    public class UoArtService : IUoArtService
    {
        private string _artMulPath;
        private string _artIdxPath;
        private string _huesMulPath;
        private string _lightMulPath;
        private string _lightIdxPath;
        private MulFileManager _mulFileManager; // New field

        private ushort[,]? _huesData; // [hueIndex, colorIndex]

        public UoArtService(AllSettings allSettings, MulFileManager mulFileManager)
        {
            _mulFileManager = mulFileManager; // Initialize new field
            // Paths will be loaded when Load method is explicitly called from MainViewModel
            // This constructor is primarily for dependency injection
        }

        public void Load(AllSettings allSettings) // Matches IUoArtService
        {
            // Prioritize paths from FilePathsSettings
            _artMulPath = allSettings.FilePathsSettings.ArtMul;
            _artIdxPath = allSettings.FilePathsSettings.ArtIdx;
            _huesMulPath = allSettings.FilePathsSettings.HuesMul;

            // Override with paths from OverridePathsSettings if available
            var artMulItem = allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("art.mul", StringComparison.OrdinalIgnoreCase));
            var artIdxItem = allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("artidx.mul", StringComparison.OrdinalIgnoreCase));
            var huesMulItem = allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("hues.mul", StringComparison.OrdinalIgnoreCase));

            if (artMulItem != null && !string.IsNullOrEmpty(artMulItem.FilePath)) _artMulPath = artMulItem.FilePath;
            if (artIdxItem != null && !string.IsNullOrEmpty(artIdxItem.FilePath)) _artIdxPath = artIdxItem.FilePath;
            if (huesMulItem != null && !string.IsNullOrEmpty(huesMulItem.FilePath)) _huesMulPath = huesMulItem.FilePath;

            var lightMulItem = allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("light.mul", StringComparison.OrdinalIgnoreCase));
            if (lightMulItem != null && !string.IsNullOrEmpty(lightMulItem.FilePath)) _lightMulPath = lightMulItem.FilePath;

            var lightIdxItem = allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("lightidx.mul", StringComparison.OrdinalIgnoreCase));
            if (lightIdxItem != null && !string.IsNullOrEmpty(lightIdxItem.FilePath)) _lightIdxPath = lightIdxItem.FilePath;

            LoadHuesData();
        }

        private void LoadHuesData()
        {
            if (string.IsNullOrEmpty(_huesMulPath) || !File.Exists(_huesMulPath))
            {
                //Logger.Log($"Error: Hues.mul path is not configured or file does not exist. Path: {_huesMulPath}");
                _huesData = null;
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(_huesMulPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int hueGroups = (int)(stream.Length / 708);
                    _huesData = new ushort[hueGroups * 8, 32];

                    for (int i = 0; i < hueGroups; i++)
                    {
                        reader.ReadInt32(); // Header
                        for (int j = 0; j < 8; j++)
                        {
                            int hueIndex = i * 8 + j;
                            for (int k = 0; k < 32; k++)
                            {
                                _huesData[hueIndex, k] = reader.ReadUInt16();
                            }
                            reader.ReadBytes(24); // Skip TableStart, TableEnd, and Name
                        }
                    }
                }
                //Logger.Log($"Hues data loaded successfully from: {_huesMulPath}");
            }
            catch (Exception ex)
            {
                //Logger.Log($"Error loading hues data from {_huesMulPath}: {ex.Message}");
                _huesData = null;
            }
        }

        public System.Windows.Media.Color GetColorFromHue(ushort color565)
        {
            byte r = (byte)(((color565 >> 10) & 0x1F) * 255 / 31);
            byte g = (byte)(((color565 >> 5) & 0x1F) * 255 / 31);
            byte b = (byte)((color565 & 0x1F) * 255 / 31);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

        public System.Windows.Media.Color GetColorFromHue(int hue, int shade)
        {
            if (_huesData == null || hue <= 0 || shade < 0 || hue > _huesData.GetLength(0) || shade >= _huesData.GetLength(1))
            {
                //Logger.Log($"Warning: Invalid hue ({hue}) or shade ({shade}) for GetColorFromHue. HuesData is null: {_huesData == null}. Returning Transparent.");
                return Colors.Transparent; // Return a default/error color
            }
            // UO hues are 1-indexed in client, but 0-indexed in our array
            ushort color565 = _huesData[hue - 1, shade];
            return GetColorFromHue(color565);
        }

        public System.Windows.Media.Color GetColorFromDrawConfig(ushort drawConfigId)
        {
            int drawMode = drawConfigId + 19;
            float r = 1.0f, g = 1.0f, b = 1.0f;

            switch (drawMode)
            {
                case 20: r = 1.0f; g = 0.0f; b = 0.0f; break;
                case 21: r = 0.9f; g = 0.0f; b = 0.0f; break;
                case 22: r = 0.8f; g = 0.0f; b = 0.0f; break;
                case 23: r = 0.7f; g = 0.0f; b = 0.0f; break;
                case 24: r = 0.6f; g = 0.0f; b = 0.0f; break;
                case 25: r = 0.5f; g = 0.0f; b = 0.0f; break;
                case 26: r = 0.4f; g = 0.0f; b = 0.0f; break;
                case 27: r = 0.3f; g = 0.0f; b = 0.0f; break;
                case 28: r = 0.2f; g = 0.0f; b = 0.0f; break;
                case 29: r = 0.1f; g = 0.0f; b = 0.0f; break;
                case 30: r = 1.0f; g = 0.5f; b = 0.0f; break;
                case 31: r = 0.9f; g = 0.45f; b = 0.0f; break;
                case 32: r = 0.8f; g = 0.4f; b = 0.0f; break;
                case 33: r = 0.7f; g = 0.35f; b = 0.0f; break;
                case 34: r = 0.6f; g = 0.3f; b = 0.0f; break;
                case 35: r = 0.5f; g = 0.25f; b = 0.0f; break;
                case 36: r = 0.4f; g = 0.2f; b = 0.0f; break;
                case 37: r = 0.3f; g = 0.15f; b = 0.0f; break;
                case 38: r = 0.2f; g = 0.1f; b = 0.0f; break;
                case 39: r = 0.1f; g = 0.05f; b = 0.0f; break;
                case 40: r = 1.0f; g = 1.0f; b = 0.0f; break;
                case 41: r = 0.9f; g = 0.9f; b = 0.0f; break;
                case 42: r = 0.8f; g = 0.8f; b = 0.0f; break;
                case 43: r = 0.7f; g = 0.7f; b = 0.0f; break;
                case 44: r = 0.6f; g = 0.6f; b = 0.0f; break;
                case 45: r = 0.5f; g = 0.5f; b = 0.0f; break;
                case 46: r = 0.4f; g = 0.4f; b = 0.0f; break;
                case 47: r = 0.3f; g = 0.3f; b = 0.0f; break;
                case 48: r = 0.2f; g = 0.2f; b = 0.0f; break;
                case 49: r = 0.1f; g = 0.1f; b = 0.0f; break;
                case 50: r = 0.0f; g = 1.0f; b = 0.0f; break;
                case 51: r = 0.0f; g = 0.9f; b = 0.0f; break;
                case 52: r = 0.0f; g = 0.8f; b = 0.0f; break;
                case 53: r = 0.0f; g = 0.7f; b = 0.0f; break;
                case 54: r = 0.0f; g = 0.6f; b = 0.0f; break;
                case 55: r = 0.0f; g = 0.5f; b = 0.0f; break;
                case 56: r = 0.0f; g = 0.4f; b = 0.0f; break;
                case 57: r = 0.0f; g = 0.3f; b = 0.0f; break;
                case 58: r = 0.0f; g = 0.2f; b = 0.0f; break;
                case 59: r = 0.0f; g = 0.1f; b = 0.0f; break;
                case 60: r = 0.0f; g = 1.0f; b = 0.0f; break;
                case 61: r = 0.0f; g = 0.9f; b = 0.0f; break;
                case 62: r = 0.0f; g = 0.8f; b = 0.0f; break;
                case 63: r = 0.0f; g = 0.7f; b = 0.0f; break;
                case 64: r = 0.0f; g = 0.6f; b = 0.0f; break;
                case 65: r = 0.0f; g = 0.5f; b = 0.0f; break;
                case 66: r = 0.0f; g = 0.4f; b = 0.0f; break;
                case 67: r = 0.0f; g = 0.3f; b = 0.0f; break;
                case 68: r = 0.0f; g = 0.2f; b = 0.0f; break;
                case 69: r = 0.0f; g = 0.1f; b = 0.0f; break;
                case 70: r = 0.0f; g = 1.0f; b = 1.0f; break;
                case 71: r = 0.0f; g = 0.9f; b = 0.9f; break;
                case 72: r = 0.0f; g = 0.8f; b = 0.8f; break;
                case 73: r = 0.0f; g = 0.7f; b = 0.7f; break;
                case 74: r = 0.0f; g = 0.6f; b = 0.6f; break;
                case 75: r = 0.0f; g = 0.5f; b = 0.5f; break;
                case 76: r = 0.0f; g = 0.4f; b = 0.4f; break;
                case 77: r = 0.0f; g = 0.3f; b = 0.3f; break;
                case 78: r = 0.0f; g = 0.2f; b = 0.2f; break;
                case 79: r = 0.0f; g = 0.1f; b = 0.1f; break;
                case 80: r = 0.0f; g = 0.0f; b = 1.0f; break;
                case 81: r = 0.0f; g = 0.0f; b = 0.9f; break;
                case 82: r = 0.0f; g = 0.0f; b = 0.8f; break;
                case 83: r = 0.0f; g = 0.0f; b = 0.7f; break;
                case 84: r = 0.0f; g = 0.0f; b = 0.6f; break;
                case 85: r = 0.0f; g = 0.0f; b = 0.5f; break;
                case 86: r = 0.0f; g = 0.0f; b = 0.4f; break;
                case 87: r = 0.0f; g = 0.0f; b = 0.3f; break;
                case 88: r = 0.0f; g = 0.0f; b = 0.2f; break;
                case 89: r = 0.0f; g = 0.0f; b = 0.1f; break;
                case 90: r = 0.5f; g = 0.0f; b = 1.0f; break;
                case 91: r = 0.45f; g = 0.0f; b = 0.9f; break;
                case 92: r = 0.4f; g = 0.0f; b = 0.8f; break;
                case 93: r = 0.35f; g = 0.0f; b = 0.7f; break;
                case 94: r = 0.3f; g = 0.0f; b = 0.6f; break;
                case 95: r = 0.25f; g = 0.0f; b = 0.5f; break;
                case 96: r = 0.2f; g = 0.0f; b = 0.4f; break;
                case 97: r = 0.15f; g = 0.0f; b = 0.3f; break;
                case 98: r = 0.1f; g = 0.0f; b = 0.2f; break;
                case 99: r = 0.05f; g = 0.0f; b = 0.1f; break;
                case 100: r = 1.0f; g = 0.0f; b = 1.0f; break;
                case 101: r = 0.9f; g = 0.0f; b = 0.9f; break;
                case 102: r = 0.8f; g = 0.0f; b = 0.8f; break;
                case 103: r = 0.7f; g = 0.0f; b = 0.7f; break;
                case 104: r = 0.6f; g = 0.0f; b = 0.6f; break;
                case 105: r = 0.5f; g = 0.0f; b = 0.5f; break;
                case 106: r = 0.4f; g = 0.0f; b = 0.4f; break;
                case 107: r = 0.3f; g = 0.0f; b = 0.3f; break;
                case 108: r = 0.2f; g = 0.0f; b = 0.2f; break;
                case 109: r = 0.1f; g = 0.0f; b = 0.1f; break;
                case 110: r = 0.5f; g = 0.5f; b = 0.5f; break;
                case 111: r = 0.4f; g = 0.4f; b = 0.4f; break;
                case 112: r = 0.3f; g = 0.3f; b = 0.3f; break;
                case 113: r = 0.2f; g = 0.2f; b = 0.2f; break;
                case 114: r = 0.1f; g = 0.1f; b = 0.1f; break;
                case 115: r = 0.6f; g = 0.6f; b = 0.6f; break;
                case 116: r = 0.7f; g = 0.7f; b = 0.7f; break;
                case 117: r = 0.8f; g = 0.8f; b = 0.8f; break;
                case 118: r = 0.9f; g = 0.9f; b = 0.9f; break;
                case 119: r = 1.0f; g = 1.0f; b = 1.0f; break;
                case 120: r = 0.7f; g = 0.0f; b = 0.0f; break;
                default:
                    return GetColorFromHue(drawConfigId);
            }

            return System.Windows.Media.Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public BitmapSource? GetItemArt(int itemID, int hue)
        {
            // Call MulFileManager.GetArtRecord directly
            var artRecord = _mulFileManager.GetArtRecord((uint)itemID);
            if (artRecord != null)
            {
                return _mulFileManager.CreateBitmapSource(artRecord, hue);
            }
            Logger.Log($"[GetItemArt] No ArtRecord found for itemID: {itemID}. Returning null.");
            return null;
        }

        public BitmapSource? GetNpcArt(int npcID, int hue)
        {
            // Use MulFileManager to get NPC animation art
            // Assuming default direction, action, and frame for a static preview
            int direction = 0; // Example: facing south
            int action = 0;    // Example: standing still
            int frame = 0;     // Example: first frame of animation
            short artType = 0; // Placeholder, might need to be determined dynamically

            var bitmapSource = _mulFileManager.GetBodyAnimation((uint)npcID, direction, action, frame, artType, hue);
            if (bitmapSource == null)
            {
                Logger.Log($"[GetNpcArt] No BodyAnimation found for npcID: {npcID}. Returning null.");
            }
            return bitmapSource;
        }

        public unsafe BitmapSource? GetLightImage(ushort lightId, ushort colorId)
        {
            Logger.Log($"[GetLightImage] Attempting to load light image for ID: {lightId} with Color ID: {colorId}");
            Logger.Log($"[GetLightImage] Light MUL Path: {_lightMulPath}");
            Logger.Log($"[GetLightImage] Light IDX Path: {_lightIdxPath}");
            Logger.Log($"[GetLightImage] Light MUL Exists: {File.Exists(_lightMulPath)}");
            Logger.Log($"[GetLightImage] Light IDX Exists: {File.Exists(_lightIdxPath)}");

            if (string.IsNullOrEmpty(_lightMulPath) || !File.Exists(_lightMulPath) ||
                string.IsNullOrEmpty(_lightIdxPath) || !File.Exists(_lightIdxPath))
            {
                Logger.Log($"Error: light.mul or lightidx.mul paths are not configured or files do not exist. Paths: Mul={_lightMulPath}, Idx={_lightIdxPath}");
                return null;
            }

            try
            {
                using (FileStream idxStream = new FileStream(_lightIdxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader idxReader = new BinaryReader(idxStream))
                using (FileStream mulStream = new FileStream(_lightMulPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader mulReader = new BinaryReader(mulStream))
                {
                    long indexOffset = (long)lightId * 12; // Each index entry is 12 bytes (lookup, length, extra)
                    Logger.Log($"[GetLightImage] Calculated indexOffset for ID {lightId}: {indexOffset}");
                    Logger.Log($"[GetLightImage] Idx Stream Length: {idxStream.Length}");

                    if (indexOffset + 12 > idxStream.Length)
                    {
                        Logger.Log($"Warning: Light ID {lightId} out of bounds in lightidx.mul. IndexOffset: {indexOffset}, IdxStreamLength: {idxStream.Length}. Returning null.");
                        return null; // ID out of bounds
                    }

                    idxReader.BaseStream.Seek(indexOffset, SeekOrigin.Begin);

                    uint lookup = idxReader.ReadUInt32();
                    uint length = idxReader.ReadUInt32();
                    uint extra = idxReader.ReadUInt32();

                    int width = (int)(extra & 0xFFFF);
                    int height = (int)((extra >> 16) & 0xFFFF);

                    Logger.Log($"[GetLightImage] ID: {lightId}, Lookup: {lookup:X}, Length: {length}, Extra: {extra:X}, Width: {width}, Height: {height}");

                    if (lookup == 0xFFFFFFFF || length == 0 || width == 0 || height == 0)
                    {
                        Logger.Log($"[GetLightImage] Invalid entry for ID {lightId}. Lookup: {lookup:X}, Length: {length}, Width: {width}, Height: {height}. Returning null.");
                        return null; // Invalid entry
                    }

                    Logger.Log($"[GetLightImage] Mul Stream Length: {mulStream.Length}");
                    if (lookup + length > mulStream.Length)
                    {
                        Logger.Log($"[GetLightImage] Data for ID {lightId} out of bounds in light.mul. Lookup: {lookup}, Length: {length}, MulStreamLength: {mulStream.Length}. Returning null.");
                        return null;
                    }

                    mulReader.BaseStream.Seek(lookup, SeekOrigin.Begin);

                    byte[] rawPixelData = mulReader.ReadBytes((int)length);

                    WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                    byte[] pixelsBgra32 = new byte[width * height * 4];

                    System.Windows.Media.Color targetColor = GetColorFromDrawConfig(colorId);

                    fixed (byte* dataPtr = rawPixelData)
                    {
                        sbyte* bindat = (sbyte*)dataPtr;
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                sbyte value = *bindat++;
                                ushort color16bit = (ushort)(((0x1f + value) << 10) + ((0x1F + value) << 5) + (0x1F + value));

                                // Convert 16-bit (ARGB1555) to 32-bit BGRA
                                byte r = (byte)(((color16bit >> 10) & 0x1F) * 255 / 31);
                                byte g = (byte)(((color16bit >> 5) & 0x1F) * 255 / 31);
                                byte b = (byte)((color16bit & 0x1F) * 255 / 31);
                                byte a = (byte)(((color16bit & 0x8000) != 0) ? 128 : 0); // Alpha bit (MSB)

                                // Apply target color to the RGB components, preserving original alpha
                                byte finalR = (byte)(r * targetColor.R / 255);
                                byte finalG = (byte)(g * targetColor.G / 255);
                                byte finalB = (byte)(b * targetColor.B / 255);

                                int pixelIndex = (y * width + x) * 4;
                                pixelsBgra32[pixelIndex + 0] = finalB; // Blue
                                pixelsBgra32[pixelIndex + 1] = finalG; // Green
                                pixelsBgra32[pixelIndex + 2] = finalR; // Red
                                pixelsBgra32[pixelIndex + 3] = a;      // Alpha
                            }
                        }
                    }

                    bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelsBgra32, width * 4, 0);
                    Logger.Log($"Successfully loaded and converted light image for ID: {lightId}");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading light image for ID {lightId} from {_lightMulPath} and {_lightIdxPath}: {ex.Message}");
                return null;
            }
        }

        private BitmapSource? GetArt(int id, int hue, string mulPath, string idxPath)
        {
            Logger.Log($"[GetArt] Attempting to get art for ID: {id}, Hue: {hue}");
            Logger.Log($"[GetArt] MUL Path: {mulPath}, IDX Path: {idxPath}");
            Logger.Log($"[GetArt] Mul Exists: {File.Exists(mulPath)}, Idx Exists: {idxPath}");

            if (string.IsNullOrEmpty(mulPath) || !File.Exists(mulPath) ||
                string.IsNullOrEmpty(idxPath) || !File.Exists(idxPath))
            {
                Logger.Log($"Error: MUL files not found or paths not configured for ID {id}.");
                return null;
            }

            try
            {
                using (FileStream idxStream = new FileStream(idxPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader idxReader = new BinaryReader(idxStream))
                using (FileStream mulStream = new FileStream(mulPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader mulReader = new BinaryReader(mulStream))
                {
                    long indexOffset = (long)id * 12;
                    Logger.Log($"[GetArt] Calculated indexOffset: {indexOffset}");
                    Logger.Log($"[GetArt] idxStream.Length: {idxStream.Length}");

                    if (indexOffset + 12 > idxStream.Length)
                    {
                        Logger.Log($"[GetArt] ID {id} out of bounds. IndexOffset: {indexOffset}, IdxStreamLength: {idxStream.Length}");
                        return null; // ID out of bounds
                    }

                    idxReader.BaseStream.Seek(indexOffset, SeekOrigin.Begin);

                    uint lookup = idxReader.ReadUInt32();
                    uint size = idxReader.ReadUInt32();

                    Logger.Log($"[GetArt] ID: {id}, Lookup: {lookup:X}, Size: {size}");

                    if (lookup == 0xFFFFFFFF || size == 0)
                    {
                        Logger.Log($"[GetArt] Invalid entry for ID {id}. Lookup: {lookup:X}, Size: {size}");
                        return null; // Invalid entry
                    }

                    Logger.Log($"[GetArt] mulStream.Length: {mulStream.Length}");
                    if (lookup + size > mulStream.Length)
                    {
                        Logger.Log($"[GetArt] Data for ID {id} out of bounds in MUL file. Lookup: {lookup}, Size: {size}, MulStreamLength: {mulStream.Length}");
                        return null;
                    }

                    mulReader.BaseStream.Seek(lookup, SeekOrigin.Begin);

                    ushort width = mulReader.ReadUInt16();
                    ushort height = mulReader.ReadUInt16();

                    Logger.Log($"[GetArt] ID: {id}, Width: {width}, Height: {height}");

                    if (width == 0 || height == 0 || width > 1024 || height > 1024)
                    {
                        Logger.Log($"[GetArt] Invalid dimensions for ID {id}. Width: {width}, Height: {height}");
                        return null; // Invalid dimensions
                    }

                    ushort[] lineOffsets = new ushort[height];
                    for (int i = 0; i < height; i++)
                    {
                        lineOffsets[i] = mulReader.ReadUInt16();
                    }

                    WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                    byte[] pixels = new byte[width * height * 4];

                    for (int y = 0; y < height; y++)
                    {
                        mulReader.BaseStream.Seek(lookup + 4 + (height * 2) + (lineOffsets[y] * 2), SeekOrigin.Begin);

                        int x = 0;
                        while (x < width)
                        {
                            ushort xOffset = mulReader.ReadUInt16();
                            ushort runLength = mulReader.ReadUInt16();

                            if (xOffset == 0 && runLength == 0) break;

                            x += xOffset;

                            for (int i = 0; i < runLength; i++)
                            {
                                ushort color565 = mulReader.ReadUInt16();
                                System.Windows.Media.Color pixelColor;

                                if (hue > 0 && _huesData != null)
                                {
                                    byte r_555 = (byte)(color565 & 0x1F);
                                    byte g_555 = (byte)((color565 >> 5) & 0x1F);
                                    byte b_555 = (byte)((color565 >> 10) & 0x1F);
                                    int colorIndex = (r_555 + g_555 + b_555) / 3;
                                    pixelColor = GetColorFromHue(hue, colorIndex);
                                }
                                else
                                {
                                    pixelColor = GetColorFromHue(color565);
                                }

                                int pixelDataIndex = (y * width + x) * 4;
                                if (pixelDataIndex + 3 < pixels.Length)
                                {
                                    pixels[pixelDataIndex + 0] = pixelColor.B;
                                    pixels[pixelDataIndex + 1] = pixelColor.G;
                                    pixels[pixelDataIndex + 2] = pixelColor.R;
                                    pixels[pixelDataIndex + 3] = 255;
                                }
                                x++;
                            }
                        }
                    }

                    bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
                    Logger.Log($"[GetArt] Successfully loaded art for ID: {id}");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading Uo art for ID {id}: {ex.Message}");
                return null;
            }
        }
    }
}