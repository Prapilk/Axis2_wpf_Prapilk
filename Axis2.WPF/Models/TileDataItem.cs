namespace Axis2.WPF.Models
{
    public class TileDataItem
    {
        public ushort Id { get; set; }
        public ushort Quality { get; set; } // Assumed based on user input
        public uint Flags { get; set; } // Assumed based on usage in ItemTabViewModel
        public uint AnimId { get; set; } // Assumed based on usage in ItemTabViewModel

        // Placeholder constructor to allow compilation based on LightDataService usage
        public TileDataItem(object mulStruct, ushort id)
        {
            Id = id;
            // These are placeholders. Actual values would come from mulStruct.
            // The user will need to manually adjust this part if compilation errors related to TileDataItem persist.
            Quality = id; // Placeholder: assuming quality is related to ID if not explicitly in mulStruct
            Flags = 0;
            AnimId = 0;
        }
    }
}