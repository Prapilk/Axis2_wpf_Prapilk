using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;
using Axis2.WPF.Services;
using Axis2.WPF.ViewModels.Settings;

// Ajout des directives using pour les nouvelles classes
// Elles sont maintenant dans le même namespace Axis2.WPF, donc pas besoin de 'using Axis2_WPF;' si elles sont au même niveau.
// Si elles sont dans un sous-dossier, il faudrait un 'using Axis2.WPF.SubFolder;'

namespace Axis2.WPF.Services
{
    public class ArtRecord
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public ushort[] Palette { get; set; } = Array.Empty<ushort>();
    }

    public class MulFileManager
    {
        private string _artIdxPath = string.Empty;
        private string _artMulPath = string.Empty;
        private string _huesMulPath = string.Empty;
        private string _animIdxPath = string.Empty;
        private string _animMulPath = string.Empty;
        private BodyDefService _bodyDefService;
        private readonly Dictionary<string, string> _overridePaths;

        private FileManager _fileManager; // Add FileManager dependency
        private AnimationManager _animationManager; // Add AnimationManager dependency
        
        public MulFileManager(FileManager fileManager, AnimationManager animationManager, BodyDefService bodyDefService)
        {
            _fileManager = fileManager;
            _animationManager = animationManager;
            _bodyDefService = bodyDefService;
            _overridePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Load(string artIdxPath, string artMulPath, string huesMulPath, string animIdxPath, string animMulPath, BodyDefService bodyDefService, IEnumerable<FilePathItem> overridePaths)
        {
            _artIdxPath = artIdxPath;
            _artMulPath = artMulPath;
            _huesMulPath = huesMulPath;
            _animIdxPath = animIdxPath;
            _animMulPath = animMulPath;
            _bodyDefService = bodyDefService;
            LoadHues();

            _overridePaths.Clear();
            if (overridePaths != null)
            {
                foreach (var pathItem in overridePaths)
                {
                    _overridePaths[pathItem.FileName] = pathItem.FilePath;
                }
            }
        }

        private string? GetOverridePath(string fileName)
        {
            if (_overridePaths.TryGetValue(fileName, out string? path))
            {
                return path;
            }
            return null;
        }

        public ArtRecord? GetArtRecord(uint itemId)
        {
            uint finalItemId = itemId;
            if (itemId < 0x4000)
            {
                finalItemId += 0x4000;
            }
            else // itemId >= 0x4000
            {
                finalItemId += 0x4000;
            }
            
            var indexRecord = ReadIndexRecord(_artIdxPath, finalItemId);
            if (indexRecord == null || indexRecord.Lookup == 0xFFFFFFFF || indexRecord.Size <= 0)
            {
                return null;
            }

            return ReadItemData(_artMulPath, indexRecord.Lookup, indexRecord.Size);
        }


                private ArtRecord? ReadItemData(string artPath, uint lookup, int size)
        {
            if (!File.Exists(artPath)) return null;
            try
            {
                using (var fs = new FileStream(artPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (lookup + size > fs.Length) return null;

                    byte[] bData = new byte[size];
                    fs.Seek(lookup, SeekOrigin.Begin);
                    fs.Read(bData, 0, size);

                    int dwOffset = 4;
                    ushort wArtWidth = BitConverter.ToUInt16(bData, dwOffset);
                    dwOffset += 2;
                    ushort wArtHeight = BitConverter.ToUInt16(bData, dwOffset);
                    dwOffset += 2;

                    if (wArtWidth == 0 || wArtWidth > 1024 || wArtHeight == 0 || wArtHeight > 1024) return null;

                    ushort[] lineStart = new ushort[wArtHeight];
                    for (int i = 0; i < wArtHeight; i++)
                    {
                        lineStart[i] = BitConverter.ToUInt16(bData, dwOffset);
                        dwOffset += 2;
                    }

                    int dataStart = dwOffset;
                    byte[] imageData = new byte[wArtWidth * wArtHeight * 2];
                    int y = 0;
                    while (y < wArtHeight)
                    {
                        dwOffset = lineStart[y] * 2 + dataStart;
                        int x = 0;
                        
                        while (true)
                        {
                            ushort xOffset = BitConverter.ToUInt16(bData, dwOffset);
                            dwOffset += 2;
                            ushort xRun = BitConverter.ToUInt16(bData, dwOffset);
                            dwOffset += 2;

                            if (xOffset == 0 && xRun == 0)
                            {
                                break;
                            }

                            x += xOffset;
                            for (int run = 0; run < xRun; run++)
                            {
                                ushort pixel = BitConverter.ToUInt16(bData, dwOffset);
                                dwOffset += 2;
                                int dataIndex = (y * wArtWidth + x) * 2;
                                if (dataIndex + 1 < imageData.Length)
                                {
                                    imageData[dataIndex] = (byte)(pixel & 0xFF);
                                    imageData[dataIndex + 1] = (byte)(pixel >> 8);
                                }
                                x++;
                            }
                        }
                        y++;
                    }
                    return new ArtRecord { ImageData = imageData, Width = wArtWidth, Height = wArtHeight };
                }
            }
            catch { return null; }
        }

        public BitmapSource? CreateBitmapSource(ArtRecord? artRecord, int hue = 0)
        {
            if (artRecord == null || artRecord.ImageData == null) return null;

            int width = artRecord.Width;
            int height = artRecord.Height;
            byte[] imageData = artRecord.ImageData;
            bool isBgra = imageData.Length == width * height * 4;

            if (isBgra)
            {
                return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, imageData, width * 4);
            }
            else
            {
                byte[] pixels = new byte[width * height * 4]; // BGRA
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelIndex16bit = (y * width + x) * 2;
                        int pixelBGRAIndex = (y * width + x) * 4;
                        if (pixelIndex16bit + 1 >= imageData.Length) continue;
                        ushort rawPixel = (ushort)(imageData[pixelIndex16bit] | (imageData[pixelIndex16bit + 1] << 8));
                        if (rawPixel == 0)
                        {
                            pixels[pixelBGRAIndex + 3] = 0; // Transparent
                            continue;
                        }
                        uint color = BlendColors(rawPixel, hue, false);
                        byte r = (byte)((color >> 16) & 0xFF);
                        byte g = (byte)((color >> 8) & 0xFF);
                        byte b = (byte)(color & 0xFF);
                        pixels[pixelBGRAIndex] = r;
                        pixels[pixelBGRAIndex + 1] = g;
                        pixels[pixelBGRAIndex + 2] = b;
                        pixels[pixelBGRAIndex + 3] = 255;
                    }
                }
                return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            }
        }
        
        public BitmapSource? GetBodyAnimationFromUop(uint body, int direction, int action, int frame, int hue)
        {
            ushort animId = (ushort)body;
            // mulFileId est maintenant le groupIndex pour l'AnimationManager
            int groupIndex = 0;

            var bodyConv = _bodyDefService.GetBodyConv(animId);
            if (bodyConv != null)
            {
                animId = bodyConv.NewId;
                groupIndex = bodyConv.MulFile; // Utilise MulFile comme groupIndex
            }
            else
            {
                var bodyDef = _bodyDefService.GetBodyDef(animId);
                if (bodyDef != null) animId = bodyDef.NewId;
            }

            // Obtenir les données de la frame d'animation depuis AnimationManager
           // Logger.Log($"GetBodyAnimationFromUop: Requesting animation data for animId: {animId}, groupIndex: {groupIndex}, direction: {direction}");
            IndexDataFileInfo? frameInfo = _animationManager.GetAnimationFrameData(animId, groupIndex, direction);

            if (frameInfo == null || frameInfo.UopHeader.DecompressedSize == 0)
            {
                System.Windows.MessageBox.Show($"MulFileManager: Données d'animation UOP non trouvées pour ID: 0x{body:X}, Group: {groupIndex}, Direction: {direction}.", "Erreur UOP");
                return null;
            }

           // Logger.Log($"MulFileManager: Calling GetData() on frameInfo. UopHeader.DecompressedSize: {frameInfo.UopHeader.DecompressedSize}");
            byte[]? decompressedData = frameInfo.GetData();

            if (decompressedData == null)
            {
                System.Windows.MessageBox.Show($"MulFileManager: Échec de la lecture/décompression des données pour l'ID 0x{body:X}.", "Erreur UOP");
                return null;
            }

            System.Windows.MessageBox.Show($"MulFileManager: Taille des données décompressées: {decompressedData.Length}", "MulFileManager Debug");

            int width, height;
            // Utilisation de ArtDataProcessor pour traiter les pixels
            byte[]? processedPixels = ArtDataProcessor.ProcessArtPixels(decompressedData, ArtDataProcessor.ArtType.ART_NPC, (ushort)hue, out width, out height, frame);

            if (processedPixels == null)
            {
                System.Windows.MessageBox.Show($"MulFileManager: Échec du traitement des pixels pour l'ID 0x{body:X}.", "Erreur UOP");
                return null;
            }

            System.Windows.MessageBox.Show($"MulFileManager: Dimensions de l'image traitée: {width}x{height}", "MulFileManager Debug");

            // Utilisation de WpfImageHelper pour créer le BitmapSource
            BitmapSource? bitmapSource = WpfImageHelper.CreateBitmapSource(processedPixels, width, height, PixelFormats.Bgra32);

            if (bitmapSource == null)
            {
                System.Windows.MessageBox.Show($"MulFileManager: Échec de la création du BitmapSource pour l'ID 0x{body:X}.", "Erreur UOP");
            }

            return bitmapSource;
        }

        private ArtRecord? ReadAnimationDataFromBytes(byte[] data, int direction, int action, int hue)
        {
            if (data == null || data.Length == 0) return null;

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                ushort[] palette = new ushort[256];
                for (int i = 0; i < 256; i++) palette[i] = br.ReadUInt16();

                uint frameCount = br.ReadUInt32();
                uint[] frameOffsets = new uint[frameCount];
                for (int i = 0; i < frameCount; i++) frameOffsets[i] = br.ReadUInt32();

                int frameIndex = action * 5 + direction;
                if (frameIndex >= frameCount) return null;

                ms.Seek((256 * 2) + frameOffsets[frameIndex], SeekOrigin.Begin);

                short imageCenterX = br.ReadInt16();
                short imageCenterY = br.ReadInt16();
                short width = br.ReadInt16();
                short height = br.ReadInt16();

                //System.Windows.MessageBox.Show($"ReadAnimationDataFromBytes: Width={width}, Height={height}", "MulFileManager Debug");

                if (width <= 0 || height <= 0) return null;

                byte[] bgraPixels = new byte[width * height * 4];
                int y = 0;
                int previousLineNum = 0xFF;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    short header = br.ReadInt16();
                    short offset = br.ReadInt16();

                    if (header == 0x7FFF && offset == 0x7FFF) break;

                    ushort runLength = (ushort)(header & 0x0FFF);
                    ushort currentLineNum = (ushort)((header >> 12) & 0x000f);
                    
                    ushort wTmp = (ushort)(offset & 0x8000);
                    offset = (short)(wTmp | (offset >> 6));

                    if (previousLineNum != 0xFF && currentLineNum != previousLineNum) y++;
                    previousLineNum = currentLineNum;

                    for (int j = 0; j < runLength; j++)
                    {
                        byte colorIndex = br.ReadByte();
                        int currentX = (action == 0) ? (width / 2) + offset + j : (width / 2) - offset - j;

                        if (currentX >= 0 && currentX < width && y >= 0 && y < height)
                        {
                            uint blendedColor = BlendColors(palette[colorIndex], hue, false);
                            byte b = (byte)(blendedColor & 0xFF);
                            byte g = (byte)((blendedColor >> 8) & 0xFF);
                            byte r = (byte)((blendedColor >> 16) & 0xFF);
                            int pixelBGRAIndex = (y * width + currentX) * 4;
                            bgraPixels[pixelBGRAIndex + 0] = r;
                            bgraPixels[pixelBGRAIndex + 1] = g;
                            bgraPixels[pixelBGRAIndex + 2] = b;
                            bgraPixels[pixelBGRAIndex + 3] = 255;

                            if (y == 0 && currentX < 10) // Sample first 10 pixels of the first line
                            {
                                //System.Windows.MessageBox.Show($"Pixel at ({currentX},{y}): R={r}, G={g}, B={b}", "MulFileManager Debug");
                            }
                        }
                    }
                }
                return new ArtRecord { Width = width, Height = height, ImageData = bgraPixels, Palette = palette };
            }
        }

        public BitmapSource? GetBodyAnimation(uint body, int direction, int action, int frame, short artType, int hue = 0)
        {
            ushort animId = (ushort)body;
            int mulFileId = 0;

            if (_bodyDefService != null)
            {
                var bodyConv = _bodyDefService.GetBodyConv(animId);
                if (bodyConv != null)
                {
                    animId = bodyConv.NewId;
                    mulFileId = bodyConv.MulFile;
                }
                else
                {
                    var bodyDef = _bodyDefService.GetBodyDef(animId);
                    if (bodyDef != null) animId = bodyDef.NewId;
                }
            }

            string animBaseDir = Path.GetDirectoryName(_animIdxPath) ?? string.Empty;
            string currentAnimIdxPath = Path.Combine(animBaseDir, $"anim{mulFileId}.idx");
            string currentAnimMulPath = Path.Combine(animBaseDir, $"anim{mulFileId}.mul");
            if (!File.Exists(currentAnimIdxPath) || !File.Exists(currentAnimMulPath))
            {
                currentAnimIdxPath = _animIdxPath;
                currentAnimMulPath = _animMulPath;
            }

            uint absoluteAnimIdxRecordId;
            if (animId < 200) absoluteAnimIdxRecordId = (uint)(animId * 110);
            else if (animId < 400) absoluteAnimIdxRecordId = (200 * 110) + (((uint)animId - 200) * 65);
            else absoluteAnimIdxRecordId = (200 * 110) + (200 * 65) + (((uint)animId - 400) * 175);
            absoluteAnimIdxRecordId += (uint)(action * 5 + direction);

            var indexRecord = ReadIndexRecord(currentAnimIdxPath, absoluteAnimIdxRecordId);
            if (indexRecord == null) return null;

            if (indexRecord.Lookup != 0xFFFFFFFF && indexRecord.Size > 0)
            {
                ArtRecord? artRecord = ReadAnimationData(currentAnimMulPath, indexRecord.Lookup, indexRecord.Size, direction, action, hue);
                if (artRecord == null) return null;
                return CreateNpcBitmapSource(artRecord, hue);
            }
            return null;
        }

        private IndexRecord? ReadIndexRecord(string indexPath, uint recordId)
        {
            if (string.IsNullOrEmpty(indexPath) || !File.Exists(indexPath)) return null;
            try
            {
                using (var fs = new FileStream(indexPath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    long position = recordId * 12;
                    if (position + 12 > fs.Length) return null;
                    fs.Seek(position, SeekOrigin.Begin);
                    var index = new IndexRecord { Lookup = br.ReadUInt32(), Size = br.ReadInt32(), Unknown = br.ReadInt32() };
                    return (index.Lookup == 0xFFFFFFFF || index.Size <= 0) ? null : index;
                }
            }
            catch { return null; }
        }

        private ArtRecord? ReadAnimationData(string animPath, uint lookup, int size, int direction, int action, int hue)
        {
            if (!File.Exists(animPath)) return null;
            try
            {
                using (var fs = new FileStream(animPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var br = new BinaryReader(fs))
                {
                    fs.Seek(lookup, SeekOrigin.Begin);
                    byte[] data = br.ReadBytes(size);
                    return ReadAnimationDataFromBytes(data, direction, action, hue);
                }
            }
            catch { return null; }
        }

        public BitmapSource? CreateNpcBitmapSource(ArtRecord artRecord, int hue = 0)
        {
            if (artRecord?.ImageData == null || artRecord.ImageData.Length == 0) return null;
            return BitmapSource.Create(artRecord.Width, artRecord.Height, 96, 96, PixelFormats.Bgra32, null, artRecord.ImageData, artRecord.Width * 4);
        }

        private uint ScaleColor(ushort wColor)
        {
            return (uint)(((((wColor >> 10) & 0x1F) * 0xFF / 0x1F)) |
                          ((((wColor >> 5) & 0x1F) * 0xFF / 0x1F) << 8) |
                          (((wColor & 0x1F) * 0xFF / 0x1F) << 16));
        }

        private uint BlendColors(ushort wBaseColor, int wAppliedColor, bool bBlendMode)
        {
            if (wAppliedColor == 0) return ScaleColor(wBaseColor);
            uint dwBase = ScaleColor(wBaseColor);
            byte r = (byte)((dwBase >> 16) & 0xFF);
            byte g = (byte)((dwBase >> 8) & 0xFF);
            byte b = (byte)(dwBase & 0xFF);
            if (bBlendMode && r != g && r != b) return dwBase;
            ushort wOutput = (ushort)(((wBaseColor >> 10) + ((wBaseColor >> 5) & 0x1F) + (wBaseColor & 0x1F)) / 3);
            if (wOutput > 31) wOutput = 31;

            if (wAppliedColor <= 0 || _hues == null || !_hues.ContainsKey(wAppliedColor)) return dwBase;

            Hue selectedHue = _hues[wAppliedColor];
            return (selectedHue?.ColorTable == null || wOutput >= selectedHue.ColorTable.Length) ? dwBase : ScaleColor(selectedHue.ColorTable[wOutput]);
        }

        private class Hue { public ushort[] ColorTable { get; } = new ushort[32]; }
        private readonly Dictionary<int, Hue> _hues = new Dictionary<int, Hue>();

        private void LoadHues()
        {
            if (_hues.Any() || string.IsNullOrEmpty(_huesMulPath) || !File.Exists(_huesMulPath))
            {
                return;
            }

            _hues.Clear();

            using (FileStream stream = new FileStream(_huesMulPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int hueGroups = (int)(stream.Length / 708);

                for (int i = 0; i < hueGroups; i++)
                {
                    reader.ReadInt32(); // Header for the group of 8 hues
                    for (int j = 0; j < 8; j++)
                    {
                        int hueIndex = (i * 8 + j) + 1; // Hues are 1-indexed
                        var hue = new Hue();
                        for (int k = 0; k < 32; k++)
                        {
                            hue.ColorTable[k] = reader.ReadUInt16();
                        }
                        _hues[hueIndex] = hue;
                        reader.BaseStream.Seek(24, SeekOrigin.Current); // Skip TableStart, TableEnd, and Name
                    }
                }
            }
        }

        private class IndexRecord { public uint Lookup; public int Size; public int Unknown; }

        private ushort[]? GetHueLookup(int hueIndex)
        {
            LoadHues();
            if (hueIndex <= 0 || _hues == null || hueIndex > _hues.Count)
            {
                return null;
            }
            return _hues[hueIndex - 1].ColorTable;
        }

        private System.Windows.Media.Color ApplyHue(System.Windows.Media.Color baseColor, int hue)
        {
            var hsv = RGB2HSV(baseColor);
            hsv.H = (hue / 360.0) % 1.0;
            return HSV2RGB(hsv);
        }

        private (double H, double S, double V) RGB2HSV(System.Windows.Media.Color rgb)
        {
            double r = rgb.R / 255.0;
            double g = rgb.G / 255.0;
            double b = rgb.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;
            double hue = 0;
            if (delta != 0)
            {
                if (max == r) hue = (g - b) / delta % 6;
                else if (max == g) hue = (b - r) / delta + 2;
                else hue = (r - g) / delta + 4;
                hue *= 60;
                if (hue < 0) hue += 360;
            }
            double saturation = (max == 0) ? 0 : delta / max;
            double value = max;
            return (hue / 360, saturation, value);
        }

        private System.Windows.Media.Color HSV2RGB((double H, double S, double V) hsv)
        {
            double r, g, b;
            if (hsv.S == 0)
            {
                r = g = b = hsv.V;
            }
            else
            {
                double h = hsv.H * 6;
                int i = (int)Math.Floor(h);
                double f = h - i;
                double p = hsv.V * (1 - hsv.S);
                double q = hsv.V * (1 - hsv.S * f);
                double t = hsv.V * (1 - hsv.S * (1 - f));
                switch (i % 6)
                {
                    case 0: r = hsv.V; g = t; b = p; break;
                    case 1: r = q; g = hsv.V; b = p; break;
                    case 2: r = p; g = hsv.V; b = t; break;
                    case 3: r = p; g = q; b = hsv.V; break;
                    case 4: r = t; g = p; b = hsv.V; break;
                    default: r = hsv.V; g = p; b = q; break;
                }
            }
            return System.Windows.Media.Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
    }
}