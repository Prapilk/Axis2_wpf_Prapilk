namespace Axis2.WPF
{
    // UopDataHeader.cs
    public struct UopDataHeader
    {
        public ulong Offset { get; set; }
        public uint HeaderSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public ulong Hash { get; set; }
        public ushort Flag { get; set; }

        public UopDataHeader(ulong offset, uint headerSize, uint compressedSize, uint decompressedSize, ulong hash, ushort flag)
        {
            Offset = offset;
            HeaderSize = headerSize;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            Hash = hash;
            Flag = flag;
        }
    }
}
