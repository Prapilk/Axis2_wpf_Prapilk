using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis2.WPF.Services
{
    public class UopFileEntry
    {
        public ulong Hash { get; set; }
        public long Offset { get; set; }
        public int CompressedSize { get; set; }
        public int DecompressedSize { get; set; }
    }

    public class UopManager
    {
        private static readonly Dictionary<string, List<UopFileEntry>> _uopCache = new Dictionary<string, List<UopFileEntry>>();

        public static ulong HashFileName(string s)
        {
            ulong hash = 0;
            s = s.ToLowerInvariant();
            foreach (char c in s)
            {
                hash = (hash << 5) + hash + c;
            }
            return hash;
        }

        public static List<UopFileEntry> ParseUopFile(string filePath)
        {
            if (_uopCache.ContainsKey(filePath))
            {
                return _uopCache[filePath];
            }

            var entries = new List<UopFileEntry>();
            if (!File.Exists(filePath))
            {
                return entries;
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var br = new BinaryReader(fs))
            {
                if (br.ReadInt32() != 0x50594D) // 'MYP'
                {
                    throw new ArgumentException("Invalid UOP file format.");
                }

                br.ReadInt32(); // version
                br.ReadInt32(); // signature
                long nextBlock = br.ReadInt64();
                br.ReadInt32(); // block capacity
                int fileCount = br.ReadInt32();

                fs.Seek(nextBlock, SeekOrigin.Begin);

                do
                {
                    int filesInBlock = br.ReadInt32();
                    nextBlock = br.ReadInt64();

                    for (int i = 0; i < filesInBlock; i++)
                    {
                        ulong hash = br.ReadUInt64();
                        long offset = br.ReadInt64();
                        int compressedSize = br.ReadInt32();
                        int decompressedSize = br.ReadInt32();
                        
                        // The offset in the file is relative to the start of the data, not the file itself.
                        // The C++ code adds dwHeaderLenght, which seems to be a constant 6 bytes of some sort.
                        // For now, we will assume the offset is correct as read.
                        // This might need adjustment based on the actual UOP variant.
                        
                        entries.Add(new UopFileEntry
                        {
                            Hash = hash,
                            Offset = offset,
                            CompressedSize = compressedSize,
                            DecompressedSize = decompressedSize
                        });

                        br.ReadUInt16(); // unknown
                    }

                    if (nextBlock == 0) break;
                    fs.Seek(nextBlock, SeekOrigin.Begin);

                } while (true);
            }

            _uopCache[filePath] = entries;
            return entries;
        }
    }
}
