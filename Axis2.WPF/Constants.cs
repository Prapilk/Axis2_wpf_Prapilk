namespace Axis2.WPF
{
    public static class Constants
    {
        public const int MAX_ANIMATIONS_DATA_INDEX_COUNT = 0x10000; // 65536
        public const int ANIMATION_UOP_GROUPS_COUNT = 52; // Arbitrary, based on typical group counts
        public const int ANIMATION_GROUPS_COUNT = 30; // Arbitrary, based on typical group counts
        public const int MAX_ANIMATION_FRAME_UOP_FILES = 5; // From C++ IFOR(fileIndex, 1, 5)

        // Item Attributes (placeholders - actual values need to be confirmed from original project)
        public const int ATTR_IDENTIFIED = 0x00001;
        public const int ATTR_DECAY = 0x00002;
        public const int ATTR_NEWBIE = 0x00004;
        public const int ATTR_MOVE_ALWAYS = 0x00008;
        public const int ATTR_MOVE_NEVER = 0x00010;
        public const int ATTR_MAGIC = 0x00020;
        public const int ATTR_OWNED = 0x00040;
        public const int ATTR_INVIS = 0x00080;
        public const int ATTR_CURSED = 0x00100;
        public const int ATTR_DAMNED = 0x00200;
        public const int ATTR_BLESSED = 0x00400;
        public const int ATTR_SACRED = 0x00800;
        public const int ATTR_FORSALE = 0x01000;
        public const int ATTR_STOLEN = 0x02000;
        public const int ATTR_CAN_DECAY = 0x04000;
        public const int ATTR_STATIC = 0x08000;

        public const long ITEM_SPAWN_ITEM = 69;
        public const long ITEM_SPAWN_CHAR = 34;
        public const int SPAWN_MESSAGE_DELAY = 300;
    }
}
