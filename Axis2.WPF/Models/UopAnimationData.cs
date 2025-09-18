using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Axis2.WPF.Models
{
    /// <summary>
    /// Represents the main header of a UOP .bin file containing animation data.
    /// </summary>
    public class UopBinHeader
    {
        public uint Magic; // Should be "AMOU" (0x554F4D41 in Little-Endian)
        public uint Version;
        public uint TotalSize;
        public uint AnimationId;
        // 16 bytes reserved/unused
        public uint FrameCount;
        public uint FrameIndexOffset; // Offset from the beginning of the file
    }

    /// <summary>
    /// Represents an entry in the frame index table within the .bin file.
    /// </summary>
    public class UopFrameIndex
    {
        public ushort Direction;
        public ushort FrameNumber;
        // 8 bytes reserved/unused
        public uint FrameDataOffset; // Offset relative to the start of this index entry
        public long StreamPosition; // The absolute position of this index entry in the stream
    }

    /// <summary>
    /// Represents the header for a single animation frame.
    /// </summary>
    public class UopFrameHeader
    {
        public short CenterX;
        public short CenterY;
        public ushort Width;
        public ushort Height;
    }

    /// <summary>
    /// A container for a fully decoded animation frame.
    /// </summary>
    public class DecodedUopFrame
    {
        public UopFrameHeader Header { get; set; } = new UopFrameHeader();
        public List<System.Windows.Media.Color> Palette { get; set; } = new List<System.Windows.Media.Color>();
        public BitmapSource Image { get; set; } = null!;
    }
}