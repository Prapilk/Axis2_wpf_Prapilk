using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace Axis2.WPF.Services
{
    public class SoundService
    {
        private const int SoundIdxEntrySize = 12; // Size of each entry in soundidx.mul
        private const int SoundNameLength = 16; // Length of the sound name in sound.mul
        private const int SoundHeaderSize = 32; // Size of the sound header before actual data

        public List<Sound> LoadSoundIndexes(string soundidxFilePath, string soundmulFilePath)
        {
            List<Sound> sounds = new List<Sound>();

            if (!File.Exists(soundidxFilePath))
            {
                Logger.Log($"WARNING: soundidx.mul not found: {soundidxFilePath}");
                return sounds;
            }

            if (!File.Exists(soundmulFilePath))
            {
                Logger.Log($"WARNING: sound.mul not found: {soundmulFilePath}");
                return sounds;
            }

            using (FileStream fsIdx = new FileStream(soundidxFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader brIdx = new BinaryReader(fsIdx))
            using (FileStream fsMul = new FileStream(soundmulFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader brMul = new BinaryReader(fsMul))
            {
                long numEntries = fsIdx.Length / SoundIdxEntrySize;

                Logger.Log($"DEBUG: Loading {numEntries} sound entries from {soundidxFilePath}");

                for (int i = 0; i < numEntries; i++)
                {
                    fsIdx.Seek(i * SoundIdxEntrySize, SeekOrigin.Begin);

                    try
                    {
                        // SOUNDIDX.MUL structure:
                        // 00 Start (DWORD)
                        // 04 Length (DWORD)
                        // 08 Index (UWORD)
                        // 0A Reserved (UWORD)

                        uint startOffset = brIdx.ReadUInt32(); // DWORD Start
                        uint length = brIdx.ReadUInt32(); // DWORD Length
                        ushort id = brIdx.ReadUInt16(); // UWORD index
                        ushort reserved = brIdx.ReadUInt16(); // UWORD reserved

                        if (length != 0xFFFFFFFFU) // Check against unsigned max value
                        {
                            Sound sound = new Sound
                            {
                                ID = id,
                                StartOffset = (int)startOffset,
                                Length = (int)length
                            };

                            // Read name from sound.mul
                            // Documentation: Original filename is CHAR[16] at offset 0 of the sound data block
                            if (startOffset >= 0 && startOffset + SoundNameLength <= fsMul.Length)
                            {
                                fsMul.Seek(startOffset, SeekOrigin.Begin);
                                byte[] nameBytes = brMul.ReadBytes(SoundNameLength);
                                string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
                                sound.Name = name;
                                //Logger.Log($"DEBUG: Sound ID {id}, Offset {startOffset}, Length {length}, Name: '{name}'");
                            }
                            else
                            {
                                Logger.Log($"WARNING: Sound ID {id}: Invalid StartOffset {startOffset} for reading name ({SoundNameLength} bytes) from sound.mul. File length: {fsMul.Length}");
                                sound.Name = "(Unnamed)";
                            }

                            sounds.Add(sound);
                        }
                        else
                        {
                            //Logger.Log($"DEBUG: Sound ID {id} has invalid length (0xFFFFFFFF), skipping.");
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        Logger.Log($"DEBUG: Reached end of soundidx.mul prematurely at entry {i}.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"ERROR: Failed to read sound entry {i} from soundidx.mul: {ex.Message}");
                    }
                }
            }

            return sounds;
        }

        // Method to extract sound data (for local playback)
        public byte[] GetSoundData(string soundmulFilePath, int startOffset, int length)
        {
            if (!File.Exists(soundmulFilePath))
            {
                Logger.Log($"ERROR: sound.mul not found for GetSoundData: {soundmulFilePath}");
                return null;
            }

            using (FileStream fsMul = new FileStream(soundmulFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader brMul = new BinaryReader(fsMul))
            {
                // The actual sound data starts after a 32-byte header
                int dataOffset = startOffset + SoundHeaderSize;
                int dataLength = length - SoundHeaderSize;

                if (dataOffset >= 0 && dataOffset + dataLength <= fsMul.Length)
                {
                    fsMul.Seek(dataOffset, SeekOrigin.Begin);
                    return brMul.ReadBytes(dataLength);
                }
                else
                {
                    Logger.Log($"ERROR: Invalid offset or length for GetSoundData: Offset={startOffset}, Length={length}, DataOffset={dataOffset}, DataLength={dataLength}, FileLength={fsMul.Length}");
                    return null;
                }
            }
        }

        public byte[] CreateWavHeader(int dataLength)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                // RIFF chunk
                bw.Write(Encoding.ASCII.GetBytes("RIFF")); // Chunk ID
                bw.Write(36 + dataLength); // Chunk Size (36 bytes for header + data length)
                bw.Write(Encoding.ASCII.GetBytes("WAVE")); // Format

                // fmt chunk
                bw.Write(Encoding.ASCII.GetBytes("fmt ")); // Subchunk ID
                bw.Write(16); // Subchunk Size
                bw.Write((ushort)1); // Audio Format (PCM)
                bw.Write((ushort)1); // Num Channels (Mono)
                bw.Write(22050); // Sample Rate
                bw.Write(22050 * 1 * 16 / 8); // Byte Rate (SampleRate * NumChannels * BitsPerSample / 8)
                bw.Write((ushort)(1 * 16 / 8)); // Block Align (NumChannels * BitsPerSample / 8)
                bw.Write((ushort)16); // Bits Per Sample

                // data chunk
                bw.Write(Encoding.ASCII.GetBytes("data")); // Subchunk ID
                bw.Write(dataLength); // Subchunk Size

                return ms.ToArray();
            }
        }
    }
}