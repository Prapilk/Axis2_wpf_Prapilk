using System;
using System.IO;
using System.Windows.Media;

namespace Axis2.WPF
{
    public static class ArtDataProcessor
    {
        public enum ArtType
        {
            ART_ITEM,
            ART_NPC,
        }

        public static byte[] ProcessArtPixels(byte[] decompressedData, ArtType artType, ushort appliedColor, out int width, out int height, int frameToProcess = 0)
        {
            width = 0;
            height = 0;
            if (decompressedData == null || decompressedData.Length == 0)
                return null;

            using (var stream = new MemoryStream(decompressedData))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    switch (artType)
                    {
                        case ArtType.ART_ITEM:
                            reader.ReadUInt32();
                            width = reader.ReadUInt16();
                            height = reader.ReadUInt16();

                            if (width == 0 || width > 1024 || height == 0 || height > 1024)
                                throw new InvalidDataException($"Dimensions d'item invalides: {width}x{height}");

                            ushort[] lineStarts = new ushort[height];
                            for (int i = 0; i < height; i++)
                            {
                                lineStarts[i] = reader.ReadUInt16();
                            }

                            byte[] pixels = new byte[width * height * 4];

                            long dataStartOffset = stream.Position;

                            for (int currentY = 0; currentY < height; currentY++)
                            {
                                stream.Seek(dataStartOffset + (lineStarts[currentY] * 2), SeekOrigin.Begin);

                                int currentX = 0;
                                while (true)
                                {
                                    ushort xOffset = reader.ReadUInt16();
                                    ushort xRun = reader.ReadUInt16();

                                    if ((xRun + xOffset) > 2048)
                                        break;

                                    if ((xRun + xOffset) != 0)
                                    {
                                        currentX += xOffset;
                                        for (int i = 0; i < xRun; i++)
                                        {
                                            ushort color16 = reader.ReadUInt16();
                                            uint color32 = ColorHelper.Color16To32(color16);

                                            int pixelIndex = (currentY * width + currentX) * 4;
                                            if (pixelIndex + 3 < pixels.Length)
                                            {
                                                pixels[pixelIndex + 0] = (byte)(color32 & 0xFF);
                                                pixels[pixelIndex + 1] = (byte)((color32 >> 8) & 0xFF);
                                                pixels[pixelIndex + 2] = (byte)((color32 >> 16) & 0xFF);
                                                pixels[pixelIndex + 3] = (color16 == 0) ? (byte)0 : (byte)0xFF;
                                            }
                                            currentX++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            return pixels;

                        case ArtType.ART_NPC:
                            ushort[] palette = new ushort[256];
                            for (int i = 0; i < 256; i++)
                            {
                                palette[i] = reader.ReadUInt16();
                            }

                            uint frameCount = reader.ReadUInt32();
                            uint[] frameOffsets = new uint[frameCount];
                            for (int i = 0; i < frameCount; i++)
                            {
                                frameOffsets[i] = reader.ReadUInt32();
                            }

                            if (frameToProcess >= frameCount)
                            {
                                throw new ArgumentOutOfRangeException($"frameToProcess ({frameToProcess}) est hors limites pour frameCount ({frameCount}).");
                            }

                            stream.Seek(256 * 2 + frameOffsets[frameToProcess], SeekOrigin.Begin);

                            short imageCenterX = reader.ReadInt16();
                            short imageCenterY = reader.ReadInt16();
                            width = reader.ReadInt16();
                            height = reader.ReadInt16();

                            if (width <= 0 || width > 2048 || height <= 0 || height > 2048)
                                throw new InvalidDataException($"Dimensions de NPC invalides: {width}x{height}");

                            byte[] npcPixels = null; // Initialisation pour satisfaire le compilateur

                        int currentNpcY = 0;
                        ushort previousLine = 0xFF; // Changé en ushort

                        while (stream.Position < stream.Length)
                        {
                            short header = reader.ReadInt16();
                            short offset = reader.ReadInt16();

                            if (header == 0x7FFF || offset == 0x7FFF)
                                break;

                            ushort runLength = (ushort)(header & 0x0FFF);
                            ushort lineNum = (ushort)((header >> 12) & 0x000f);
                            offset = (short)((offset & 0x8000) | (offset >> 6));

                            if (runLength == 0 || runLength > 2048)
                                break;

                            int currentNpcX = -imageCenterX + offset;

                            if (previousLine != 0xFF && lineNum != previousLine)
                                currentNpcY++;

                            previousLine = lineNum; // Correction ici

                            if (currentNpcY < 0 || currentNpcY >= height)
                                break;

                            // Assurez-vous que npcPixels est initialisé avant d'être utilisé
                            if (npcPixels == null) // Cette vérification est une sécurité, mais l'initialisation plus haut devrait suffire
                            {
                                npcPixels = new byte[width * height * 4];
                            }

                            for (int j = 0; j < runLength; j++)
                            {
                                byte paletteIndex = reader.ReadByte();
                                ushort color16 = palette[paletteIndex];
                                uint color32 = ColorHelper.Color16To32(color16);

                                int pixelIndex = (currentNpcY * width + currentNpcX) * 4;
                                if (pixelIndex + 3 < npcPixels.Length)
                                {
                                    npcPixels[pixelIndex + 0] = (byte)(color32 & 0xFF);
                                    npcPixels[pixelIndex + 1] = (byte)((color32 >> 8) & 0xFF);
                                    npcPixels[pixelIndex + 2] = (byte)((color32 >> 16) & 0xFF);
                                    npcPixels[pixelIndex + 3] = (color16 == 0) ? (byte)0 : (byte)0xFF;
                                }
                                currentNpcX++;
                            }
                        }
                        return npcPixels;

                        default:
                            Console.WriteLine($"ArtType {artType} non implémenté pour le traitement des pixels.");
                            return null;
                    }
                }
                catch (EndOfStreamException)
                {
                    Console.WriteLine("Fin de flux inattendue lors du traitement des pixels. Les données pourraient être tronquées ou corrompues.");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du traitement des pixels d'art: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
