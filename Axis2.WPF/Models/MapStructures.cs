namespace Axis2.WPF.Models
{
    public struct MapTile
    {
        public short TileID; // The graphic ID of the tile
        public sbyte Z;      // The Z-coordinate (altitude)
    }

    public struct MapIndexEntry
    {
        public int Offset; // Offset in map0.mul
        public int Size;   // Size of the block data
        public int Unknown; // Usually -1
    }
}