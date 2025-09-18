using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axis2.WPF.Models;
using Axis2.WPF.Mvvm; // For Logger
using Axis2.WPF.Services;

namespace Axis2.WPF
{
    public static class AnimationDataLoader
    {
        /// <summary>
        /// Découvre toutes les actions disponibles pour une animation donnée dans un fichier UOP
        /// </summary>
        public static List<int> GetAvailableActionsForAnimation(uint animId, UopFileReader uopReader)
        {
            var availableActions = new HashSet<int>();

            if (!uopReader.IsLoaded)
            {
                Logger.Log("WARNING: UOP file is not loaded.");
                return new List<int>();
            }

            // Tester les actions possibles (généralement 0-35 pour les animations UO)
            for (int action = 0; action <= 35; action++)
            {
                // Tester au moins une direction pour voir si cette action existe
                for (int direction = 0; direction < 5; direction++)
                {
                    string binPath = GetAnimationFrameUopHashPath(animId, action, direction);
                    ulong hash = UopFileReader.CreateHash(binPath);

                    var entry = uopReader.GetEntryByHash(hash);
                    if (entry.HasValue)
                    {
                        availableActions.Add(action);
                        Logger.Log($"DEBUG: Found action {action} for animation {animId}");
                        break; // On a trouvé au moins une direction pour cette action
                    }
                }
            }

            var result = availableActions.OrderBy(x => x).ToList();
            Logger.Log($"INFO: Animation {animId} has {result.Count} available actions: [{string.Join(", ", result)}]");
            return result;
        }

        /// <summary>
        /// Obtient la première action disponible pour une animation
        /// </summary>
        public static int GetFirstAvailableAction(uint animId, UopFileReader uopReader)
        {
            var actions = GetAvailableActionsForAnimation(animId, uopReader);
            return actions.Count > 0 ? actions[0] : -1;
        }

        /// <summary>
        /// Obtient les directions disponibles pour une action spécifique d'une animation
        /// </summary>
        public static List<int> GetAvailableDirectionsForAction(uint animId, int action, UopFileReader uopReader)
        {
            var availableDirections = new List<int>();

            if (!uopReader.IsLoaded)
            {
                Logger.Log("WARNING: UOP file is not loaded.");
                return availableDirections;
            }

            for (int direction = 0; direction < 5; direction++)
            {
                string binPath = GetAnimationFrameUopHashPath(animId, action, direction);
                ulong hash = UopFileReader.CreateHash(binPath);

                var entry = uopReader.GetEntryByHash(hash);
                if (entry.HasValue)
                {
                    availableDirections.Add(direction);
                }
            }

            Logger.Log($"DEBUG: Animation {animId}, Action {action} has directions: [{string.Join(", ", availableDirections)}]");
            return availableDirections;
        }

        /// <summary>
        /// Charge une frame d'animation avec fallback intelligent
        /// </summary>
        public static DecodedUopFrame? LoadAnimationFrameWithFallback(uint animId, int preferredAction, int direction, UopFileReader uopReader)
        {
            if (!uopReader.IsLoaded)
            {
                Logger.Log("ERROR: UOP file is not loaded.");
                return null;
            }

            // Essayer d'abord l'action préférée
            var frame = TryLoadAnimationFrame(animId, preferredAction, direction, uopReader);
            if (frame != null)
            {
                Logger.Log($"SUCCESS: Loaded animation {animId}, action {preferredAction}, direction {direction}");
                return frame;
            }

            // Si l'action préférée n'existe pas, chercher la première disponible
            Logger.Log($"INFO: Action {preferredAction} not found for animation {animId}, searching for alternatives...");

            int firstAvailableAction = GetFirstAvailableAction(animId, uopReader);
            if (firstAvailableAction >= 0 && firstAvailableAction != preferredAction)
            {
                Logger.Log($"INFO: Using action {firstAvailableAction} instead of {preferredAction} for animation {animId}");

                frame = TryLoadAnimationFrame(animId, firstAvailableAction, direction, uopReader);
                if (frame != null)
                {
                    return frame;
                }

                // Si la direction demandée n'existe pas, essayer la première direction disponible
                var availableDirections = GetAvailableDirectionsForAction(animId, firstAvailableAction, uopReader);
                if (availableDirections.Count > 0)
                {
                    int firstDirection = availableDirections[0];
                    Logger.Log($"INFO: Direction {direction} not found, using direction {firstDirection} for animation {animId}, action {firstAvailableAction}");

                    frame = TryLoadAnimationFrame(animId, firstAvailableAction, firstDirection, uopReader);
                    if (frame != null)
                    {
                        return frame;
                    }
                }
            }

            Logger.Log($"ERROR: No animation data found for animation {animId}");
            return null;
        }

        /// <summary>
        /// Essaie de charger une frame d'animation spécifique
        /// </summary>
        private static DecodedUopFrame? TryLoadAnimationFrame(uint animId, int action, int direction, UopFileReader uopReader)
        {
            try
            {
                string binPath = GetAnimationFrameUopHashPath(animId, action, direction);
                ulong hash = UopFileReader.CreateHash(binPath);

                var entry = uopReader.GetEntryByHash(hash);
                if (!entry.HasValue)
                {
                    Logger.Log($"DEBUG: No entry found for path: {binPath} (hash: {hash:X16})");
                    return null;
                }

                byte[]? binData = uopReader.ReadData(entry.Value);
                if (binData == null)
                {
                    Logger.Log($"WARNING: Failed to read data for {binPath}");
                    return null;
                }

                return LoadFromUopBin(binData, direction);
            }
            catch (Exception ex)
            {
                Logger.Log($"ERROR: Exception loading animation {animId}, action {action}, direction {direction}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Méthode utilitaire pour lister toutes les animations disponibles dans un fichier UOP
        /// </summary>
        public static Dictionary<uint, List<int>> DiscoverAllAnimations(UopFileReader uopReader)
        {
            var animations = new Dictionary<uint, List<int>>();

            if (!uopReader.IsLoaded)
            {
                Logger.Log("WARNING: UOP file is not loaded.");
                return animations;
            }

            Logger.Log("INFO: Discovering all animations in UOP file...");

            // Parcourir toutes les entrées du fichier UOP
            foreach (var kvp in uopReader.GetAllEntries())
            {
                var hash = kvp.Key;
                var entry = kvp.Value;

                // Essayer de décoder le chemin depuis le hash (difficile, mais on peut essayer de deviner)
                // Pour l'instant, on va tester les IDs d'animation courants
                for (uint animId = 0; animId <= 4000; animId++) // Ajustez la limite selon vos besoins
                {
                    var actions = GetAvailableActionsForAnimation(animId, uopReader);
                    if (actions.Count > 0)
                    {
                        animations[animId] = actions;
                        if (animations.Count % 100 == 0)
                        {
                            Logger.Log($"INFO: Discovered {animations.Count} animations so far...");
                        }
                    }
                }
            }

            Logger.Log($"INFO: Discovery complete. Found {animations.Count} animations total.");
            return animations;
        }

        // Vos méthodes existantes restent inchangées
        public static uint GetNpcAnimationIndex(uint npcArtId, int iMul = 1, int frame = 0)
        {
            // ... code existant inchangé
            uint dwIndex = 0;
            uint dwArtIndex = npcArtId;

            switch (iMul)
            {
                case 1:
                    if (dwArtIndex < 0xC8)
                        dwIndex = dwArtIndex * 110;
                    else if (dwArtIndex < 0x190)
                        dwIndex = (dwArtIndex - 0xC8) * 65 + 22000;
                    else
                        dwIndex = (dwArtIndex - 0x190) * 175 + 35000;
                    break;
                case 2:
                    if (dwArtIndex == 0x44)
                        dwIndex = 13420;
                    else if (dwArtIndex < 0xC8)
                        dwIndex = dwArtIndex * 110;
                    else
                        dwIndex = (dwArtIndex - 0xC8) * 65 + 22000;
                    break;
                case 3:
                    if (dwArtIndex == 0x5F)
                        dwIndex = 15175;
                    else if (dwArtIndex < 0x190)
                        dwIndex = dwArtIndex * 110;
                    else if (dwArtIndex < 0x258)
                        dwIndex = (dwArtIndex - 0x258) * 65 + 44000;
                    else
                        dwIndex = (dwArtIndex - 0x258) * 175 + 70000;
                    break;
                case 4:
                    if (dwArtIndex < 0xC8)
                        dwIndex = dwArtIndex * 110;
                    else if (dwArtIndex < 0x190)
                        dwIndex = (dwArtIndex - 0xC8) * 65 + 22000;
                    else
                        dwIndex = (dwArtIndex - 0x190) * 175 + 35000;
                    break;
                case 5:
                    if (dwArtIndex == 0x22)
                        dwIndex = 11210;
                    else if (dwArtIndex < 0xC8)
                        dwIndex = dwArtIndex * 110;
                    else if (dwArtIndex < 0x190)
                        dwIndex = (dwArtIndex - 0xC8) * 65 + 22000;
                    else
                        dwIndex = (dwArtIndex - 0x190) * 175 + 35000;
                    break;
                default:
                    Logger.Log($"WARNING: GetNpcAnimationIndex: iMul inconnu: {iMul}. Utilisation de la logique par défaut.");
                    dwIndex = dwArtIndex * 110;
                    break;
            }

            if (frame > 0)
                dwIndex += (uint)frame;

            return dwIndex;
        }

        public static string GetAnimationFrameUopHashPath(uint animId, int action, int direction)
        {
            int groupIndex = action * 5 + direction;
            return $"build/animationlegacyframe/{animId:D6}/{groupIndex:D2}.bin";
        }

        public static DecodedUopFrame? LoadFromUopBin(byte[] binData, int direction, int frameIndex = 0)
        {
            Logger.Log($"DEBUG: LoadFromUopBin called with binData.Length: {binData?.Length ?? 0}, direction: {direction}, frameIndex: {frameIndex}");

            if (binData == null || binData.Length == 0)
            {
                Logger.Log("WARNING: LoadFromUopBin: binData is null or empty.");
                return null;
            }

            using (var stream = new MemoryStream(binData))
            using (var reader = new BinaryReader(stream))
            {
                UopBinHeader header;
                try
                {
                    header = ReadUopBinHeader(reader);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: LoadFromUopBin: Error reading UOP BIN header: {ex.Message}");
                    return null;
                }

                Logger.Log($"DEBUG: LoadFromUopBin: Header Magic: {header.Magic:X}, Version: {header.Version}, TotalSize: {header.TotalSize}, AnimId: {header.AnimationId}, FrameCount: {header.FrameCount}, FrameIndexOffset: {header.FrameIndexOffset}");

                if (header.Magic != 0x554F4D41)
                {
                    Logger.Log($"WARNING: LoadFromUopBin: Invalid Magic number: {header.Magic:X}. Expected 0x554F4D41.");
                    return null;
                }

                if (header.FrameCount == 0)
                {
                    Logger.Log("WARNING: LoadFromUopBin: FrameCount is 0.");
                    return null;
                }

                // CORRECTION: Le fichier .bin contient une action complète (5 directions)
                // Calculer quelle frame charger selon la direction demandée
                uint framesPerDirection = header.FrameCount / 5;
                uint frameIndexToLoad = (uint)direction * framesPerDirection + (uint)frameIndex;

                Logger.Log($"DEBUG: LoadFromUopBin: framesPerDirection: {framesPerDirection}, direction: {direction}, frameIndex: {frameIndex}, frameIndexToLoad: {frameIndexToLoad}");

                List<UopFrameIndex> frameIndexEntries;
                try
                {
                    frameIndexEntries = ReadFrameIndex(reader, header);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: LoadFromUopBin: Error reading frame index: {ex.Message}");
                    return null;
                }
                Logger.Log($"DEBUG: LoadFromUopBin: Read {frameIndexEntries.Count} frame index entries.");

                if (frameIndexToLoad >= frameIndexEntries.Count)
                {
                    Logger.Log($"WARNING: LoadFromUopBin: frameIndexToLoad ({frameIndexToLoad}) is out of bounds for frameIndexEntries.Count ({frameIndexEntries.Count}).");
                    return null;
                }

                UopFrameIndex targetFrameIndex = frameIndexEntries[(int)frameIndexToLoad];
                Logger.Log($"DEBUG: LoadFromUopBin: TargetFrameIndex - Direction: {targetFrameIndex.Direction}, FrameNumber: {targetFrameIndex.FrameNumber}, StreamPosition: {targetFrameIndex.StreamPosition}, FrameDataOffset: {targetFrameIndex.FrameDataOffset}");

                long frameDataOffset = targetFrameIndex.StreamPosition + targetFrameIndex.FrameDataOffset;
                stream.Seek(frameDataOffset, SeekOrigin.Begin);
                Logger.Log($"DEBUG: LoadFromUopBin: Seeking to frameDataOffset: {frameDataOffset}");

                List<System.Windows.Media.Color> palette;
                try
                {
                    palette = ReadPalette(reader);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: LoadFromUopBin: Error reading palette: {ex.Message}");
                    return null;
                }
                Logger.Log($"DEBUG: LoadFromUopBin: Read {palette.Count} palette entries.");

                UopFrameHeader frameHeader;
                try
                {
                    frameHeader = ReadFrameHeader(reader);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: LoadFromUopBin: Error reading frame header: {ex.Message}");
                    return null;
                }
                Logger.Log($"DEBUG: LoadFromUopBin: FrameHeader - CenterX: {frameHeader.CenterX}, CenterY: {frameHeader.CenterY}, Width: {frameHeader.Width}, Height: {frameHeader.Height}");

                if (frameHeader.Width <= 0 || frameHeader.Height <= 0)
                {
                    Logger.Log($"WARNING: LoadFromUopBin: Invalid frame dimensions: Width={frameHeader.Width}, Height={frameHeader.Height}.");
                    return null;
                }

                BitmapSource? image;
                try
                {
                    image = DecodeRleFrame(reader, frameHeader, palette);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: LoadFromUopBin: Error decoding RLE frame: {ex.Message}");
                    return null;
                }

                if (image == null)
                {
                    Logger.Log("WARNING: LoadFromUopBin: DecodeRleFrame returned null.");
                    return null;
                }

                Logger.Log("DEBUG: LoadFromUopBin: Successfully decoded UOP frame.");
                return new DecodedUopFrame { Header = frameHeader, Palette = palette, Image = image };
            }
        }

        private static UopBinHeader ReadUopBinHeader(BinaryReader reader)
        {
            var header = new UopBinHeader
            {
                Magic = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                TotalSize = reader.ReadUInt32(),
                AnimationId = reader.ReadUInt32(),
            };
            reader.ReadBytes(16); // Sauter 16 octets réservés
            header.FrameCount = reader.ReadUInt32();
            header.FrameIndexOffset = reader.ReadUInt32();
            return header;
        }

        private static List<UopFrameIndex> ReadFrameIndex(BinaryReader reader, UopBinHeader header)
        {
            var entries = new List<UopFrameIndex>();
            reader.BaseStream.Seek(header.FrameIndexOffset, SeekOrigin.Begin);

            for (int i = 0; i < header.FrameCount; i++)
            {
                long currentStreamPos = reader.BaseStream.Position;
                var entry = new UopFrameIndex
                {
                    Direction = reader.ReadUInt16(),
                    FrameNumber = reader.ReadUInt16(),
                    StreamPosition = currentStreamPos
                };
                reader.ReadBytes(8); // Sauter 8 octets réservés
                entry.FrameDataOffset = reader.ReadUInt32();
                entries.Add(entry);
            }
            return entries;
        }

        private static List<System.Windows.Media.Color> ReadPalette(BinaryReader reader)
        {
            var palette = new List<System.Windows.Media.Color>(256);
            for (int i = 0; i < 256; i++)
            {
                ushort color555 = reader.ReadUInt16();
                byte r = (byte)(((color555 >> 10) & 0x1F) << 3);
                byte g = (byte)(((color555 >> 5) & 0x1F) << 3);
                byte b = (byte)((color555 & 0x1F) << 3);
                palette.Add(System.Windows.Media.Color.FromRgb(r, g, b));
            }
            return palette;
        }

        private static UopFrameHeader ReadFrameHeader(BinaryReader reader)
        {
            return new UopFrameHeader
            {
                CenterX = reader.ReadInt16(),
                CenterY = reader.ReadInt16(),
                Width = reader.ReadUInt16(),
                Height = reader.ReadUInt16()
            };
        }

        private static BitmapSource? DecodeRleFrame(BinaryReader reader, UopFrameHeader frameHeader, List<System.Windows.Media.Color> palette)
        {
            Logger.Log($"DEBUG: DecodeRleFrame called. Width: {frameHeader.Width}, Height: {frameHeader.Height}");

            if (frameHeader.Width <= 0 || frameHeader.Height <= 0)
            {
                Logger.Log("WARNING: DecodeRleFrame: Invalid frame dimensions (<=0).");
                return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);
            }

            var bitmap = new WriteableBitmap(frameHeader.Width, frameHeader.Height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[frameHeader.Width * frameHeader.Height];

            int centerX = frameHeader.CenterX;
            int centerY = frameHeader.CenterY;

            try
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint rleHeader = reader.ReadUInt32();
                    Logger.Log($"DEBUG: DecodeRleFrame: Read RLE Header: {rleHeader:X}");

                    if (rleHeader == 0x7FFF7FFF) // Marqueur de fin
                    {
                        Logger.Log("DEBUG: DecodeRleFrame: End marker found.");
                        break;
                    }

                    int runLength = (int)(rleHeader & 0x0FFF);
                    int y = (int)((rleHeader >> 12) & 0x03FF);
                    int x = (int)((rleHeader >> 22) & 0x03FF);

                    Logger.Log($"DEBUG: DecodeRleFrame: runLength: {runLength}, x: {x}, y: {y}");

                    // Gérer les décalages signés pour X et Y (10-bit)
                    if ((x & 0x200) != 0)
                        x = -(0x400 - x);
                    if ((y & 0x200) != 0)
                        y = -(0x400 - y);

                    for (int i = 0; i < runLength; i++)
                    {
                        byte paletteIndex = reader.ReadByte();
                        System.Windows.Media.Color color = palette[paletteIndex];
                        uint pixelColor = (uint)((255 << 24) | (color.R << 16) | (color.G << 8) | color.B);

                        // Calculer les coordonnées finales en utilisant le centre comme pivot
                        int finalX = centerX + x + i;
                        int finalY = frameHeader.Height - 1 - (-y - centerY);

                        if (finalX >= 0 && finalX < frameHeader.Width && finalY >= 0 && finalY < frameHeader.Height)
                        {
                            pixels[finalY * frameHeader.Width + finalX] = pixelColor;
                        }
                    }
                }

                bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, frameHeader.Width, frameHeader.Height), pixels, frameHeader.Width * 4, 0);
                bitmap.Freeze();
            }
            catch (Exception ex)
            {
                Logger.Log($"ERROR: DecodeRleFrame: Erreur lors du décodage de la frame RLE: {ex.Message}");
                return null;
            }

            return bitmap;
        }
    }
}