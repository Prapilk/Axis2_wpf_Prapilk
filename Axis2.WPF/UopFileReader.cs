using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression; // For ZLibStream and DeflateStream
using System.Diagnostics;
using System.Text;
using Axis2.WPF.Services;

namespace Axis2.WPF
{
    public class UopFileReader
    {
        private string _filePath;
        private Dictionary<ulong, UopDataHeader> _uopEntries;

        public string FilePath => _filePath;
        public bool IsLoaded { get; private set; }

        public UopFileReader(string filePath)
        {
            _filePath = filePath;
            _uopEntries = new Dictionary<ulong, UopDataHeader>();
            IsLoaded = false;
        }

        public bool Load()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Erreur: Fichier UOP non trouvé à {_filePath}");
                return false;
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new BinaryReader(stream))
                {
                    uint formatID = reader.ReadUInt32();
                    if (formatID != 0x0050594D) // "MYP\0" inversé
                    {
                       Console.WriteLine($"Avertissement: Le fichier UOP '{_filePath}' a un formatID inattendu: {formatID:X8}");
                        return false;
                    }

                    reader.ReadUInt32(); // formatVersion
                    reader.ReadUInt32(); // Signature?
                    ulong nextBlockOffset = reader.ReadUInt64();
                    reader.ReadUInt32(); // Block capacity?
                    reader.ReadUInt32(); // Total files count

                    while (nextBlockOffset != 0)
                    {
                        stream.Seek((long)nextBlockOffset, SeekOrigin.Begin);

                        uint countInBlock = reader.ReadUInt32();
                        nextBlockOffset = reader.ReadUInt64();

                        for (int i = 0; i < countInBlock; i++)
                        {
                            ulong offset = reader.ReadUInt64();
                            uint headerSize = reader.ReadUInt32();
                            uint compressedSize = reader.ReadUInt32();
                            uint decompressedSize = reader.ReadUInt32();
                            ulong hash = reader.ReadUInt64();
                            reader.ReadUInt32(); // unknown
                            ushort flag = reader.ReadUInt16();

                            if (offset == 0 || decompressedSize == 0)
                                continue;

                            _uopEntries[hash] = new UopDataHeader(offset, headerSize, compressedSize, decompressedSize, hash, flag);
                        }
                    }
                }
                IsLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du fichier UOP '{_filePath}': {ex.Message}");
                IsLoaded = false;
                return false;
            }
        }

        public byte[]? ReadData(UopDataHeader header)
        {
            if (!IsLoaded || header.Offset == 0 || header.DecompressedSize == 0)
            {
                Logger.Log($"[UopFileReader] ReadData returning null: IsLoaded={IsLoaded}, header.Offset={header.Offset}, header.DecompressedSize={header.DecompressedSize}");
                return null;
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // THIS IS THE FIX: Seek to the data offset PLUS the header size of the data block.
                    stream.Seek((long)header.Offset + header.HeaderSize, SeekOrigin.Begin);

                    int bytesToRead = (header.Flag == 0) ? (int)header.DecompressedSize : (int)header.CompressedSize;
                    byte[] data = new byte[bytesToRead];
                    int bytesRead = stream.Read(data, 0, data.Length);

                    if (bytesRead != data.Length)
                    {
                        Logger.Log($"[UopFileReader] ReadData returning null: bytesRead ({bytesRead}) != data.Length ({data.Length})");
                        return null;
                    }

                    if (header.Flag != 0) // Data is compressed (flag is not 0)
                    {
                        byte[] decompressedBytes = new byte[header.DecompressedSize];
                        int decompressedSize = (int)header.DecompressedSize;

                        try
                        {
                            ZLibError error = Zlib.Decompress(decompressedBytes, ref decompressedSize, data, data.Length);

                            if (error != ZLibError.Okay)
                            {
                                Logger.Log($"[UopFileReader] ReadData returning null: ZLibError = {error}");
                                return null;
                            }

                            if (decompressedSize != header.DecompressedSize)
                            {
                                Logger.Log($"[UopFileReader] ReadData returning null: decompressedSize ({decompressedSize}) != header.DecompressedSize ({header.DecompressedSize})");
                                return null;
                            }
                            return decompressedBytes;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"[UopFileReader] ReadData returning null: Decompression exception: {ex.Message}");
                            return null;
                        }
                    }
                    else // Data is not compressed (flag is 0)
                    {
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[UopFileReader] Error reading data from UOP file '{_filePath}': {ex.Message}");
                return null;
            }
        }

        public UopDataHeader? GetEntryByHash(ulong hash)
        {
            if (_uopEntries.TryGetValue(hash, out var header))
            {
                return header;
            }
            return null;
        }

        public IEnumerable<KeyValuePair<ulong, UopDataHeader>> GetAllEntries()
        {
            return _uopEntries;
        }

        public static ulong CreateHash(string s)
        {
            byte[] data = Encoding.ASCII.GetBytes(s.ToLowerInvariant());
            uint length = (uint)data.Length;
            uint len = length;
            uint a, b, c;

            a = b = c = 0xdeadbeef + len;

            int offset = 0;
            while (len > 12)
            {
                a += BitConverter.ToUInt32(data, offset);
                b += BitConverter.ToUInt32(data, offset + 4);
                c += BitConverter.ToUInt32(data, offset + 8);
                Mix(ref a, ref b, ref c);
                len -= 12;
                offset += 12;
            }

            switch (len)
            {
                case 12: c += (uint)data[offset + 11] << 24; goto case 11;
                case 11: c += (uint)data[offset + 10] << 16; goto case 10;
                case 10: c += (uint)data[offset + 9] << 8; goto case 9;
                case 9: c += data[offset + 8]; goto case 8;
                case 8: b += (uint)data[offset + 7] << 24; goto case 7;
                case 7: b += (uint)data[offset + 6] << 16; goto case 6;
                case 6: b += (uint)data[offset + 5] << 8; goto case 5;
                case 5: b += data[offset + 4]; goto case 4;
                case 4: a += (uint)data[offset + 3] << 24; goto case 3;
                case 3: a += (uint)data[offset + 2] << 16; goto case 2;
                case 2: a += (uint)data[offset + 1] << 8; goto case 1;
                case 1: a += data[offset + 0]; break;
            }

            FinalMix(ref a, ref b, ref c);

            return ((ulong)b << 32) | c;
        }

        private static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= c; a ^= Rot(c, 4); c += b;
            b -= a; b ^= Rot(a, 6); a += c;
            c -= b; c ^= Rot(b, 8); b += a;
            a -= c; a ^= Rot(c, 16); c += b;
            b -= a; b ^= Rot(a, 19); a += c;
            c -= b; c ^= Rot(b, 4); b += a;
        }

        private static void FinalMix(ref uint a, ref uint b, ref uint c)
        {
            c ^= b; c -= Rot(b, 14);
            a ^= c; a -= Rot(c, 11);
            b ^= a; b -= Rot(a, 25);
            c ^= b; c -= Rot(b, 16);
            a ^= c; a -= Rot(c, 4);
            b ^= a; b -= Rot(a, 14);
            c ^= b; c -= Rot(b, 24);
        }

        private static uint Rot(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }
    }
}