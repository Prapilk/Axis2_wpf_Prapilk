using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Axis2.WPF.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OldItemTileDataMul
    {
        public readonly uint flags;
        public readonly byte weight;
        public readonly byte quality; // This is the 'Layer'
        public readonly short miscData;
        public readonly byte unk2;
        public readonly byte quantity;
        public readonly short anim; // This is the 'AnimId'
        public readonly byte unk3;
        public readonly byte hue;
        public readonly byte stackingOffset;
        public readonly byte value;
        public readonly byte height;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewItemTileDataMul
    {
        public readonly ulong flags;
        public readonly byte weight;
        public readonly byte quality; // This is the 'Layer'
        public readonly short miscData;
        public readonly byte unk2;
        public readonly byte quantity;
        public readonly short anim; // This is the 'AnimId'
        public readonly byte unk3;
        public readonly byte hue;
        public readonly byte stackingOffset;
        public readonly byte value;
        public readonly byte height;
        public fixed byte name[20];
    }

    public class ItemTileDataItem
    {
        public ulong Flags { get; set; }
        public byte Weight { get; set; }
        public byte Quality { get; set; } // Corresponds to 'Layer'
        public short MiscData { get; set; }
        public byte Unk2 { get; set; }
        public byte Quantity { get; set; }
        public short AnimId { get; set; } // Corresponds to 'anim'
        public byte Unk3 { get; set; }
        public byte Hue { get; set; }
        public byte StackingOffset { get; set; }
        public byte Value { get; set; }
        public byte Height { get; set; }
        public string Name { get; set; } = string.Empty;
        public ushort Id { get; set; }

        public unsafe ItemTileDataItem(NewItemTileDataMul mulStruct, ushort id)
        {
            Id = id;
            Flags = mulStruct.flags;
            Weight = mulStruct.weight;
            Quality = mulStruct.quality;
            MiscData = mulStruct.miscData;
            Unk2 = mulStruct.unk2;
            Quantity = mulStruct.quantity;
            AnimId = mulStruct.anim;
            Unk3 = mulStruct.unk3;
            Hue = mulStruct.hue;
            StackingOffset = mulStruct.stackingOffset;
            Value = mulStruct.value;
            Height = mulStruct.height;
            Name = Encoding.ASCII.GetString(&mulStruct.name[0], 20).Trim('\0');
        }

        public unsafe ItemTileDataItem(OldItemTileDataMul mulStruct, ushort id)
        {
            Id = id;
            Flags = mulStruct.flags;
            Weight = mulStruct.weight;
            Quality = mulStruct.quality;
            MiscData = mulStruct.miscData;
            Unk2 = mulStruct.unk2;
            Quantity = mulStruct.quantity;
            AnimId = mulStruct.anim;
            Unk3 = mulStruct.unk3;
            Hue = mulStruct.hue;
            StackingOffset = mulStruct.stackingOffset;
            Value = mulStruct.value;
            Height = mulStruct.height;
            Name = Encoding.ASCII.GetString(&mulStruct.name[0], 20).Trim('\0');
        }
    }

    public class LightMulItem
    {
        public byte[] ColorTable { get; set; } = new byte[22];
        public int Unknown { get; set; }
        public ushort Id { get; set; }
    }
}