using System;

namespace Axis2.WPF
{
    public static class ColorHelper
    {
        public static uint Color16To32(ushort color16)
        {
            byte r = (byte)(((color16 >> 10) & 0x1F) * 0xFF / 0x1F);
            byte g = (byte)(((color16 >> 5) & 0x1F) * 0xFF / 0x1F);
            byte b = (byte)((color16 & 0x1F) * 0xFF / 0x1F);

            byte a = (color16 == 0) ? (byte)0x00 : (byte)0xFF;

            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public static uint ScaleColor(ushort wColor)
        {
            byte r = (byte)(((wColor >> 10) & 0x1F) * 0xFF / 0x1F);
            byte g = (byte)(((wColor >> 5) & 0x1F) * 0xFF / 0x1F);
            byte b = (byte)((wColor & 0x1F) * 0xFF / 0x1F);

            return (uint)((b << 16) | (g << 8) | r);
        }

        public static uint BlendColors(ushort wBaseColor, ushort wAppliedColor, bool bBlendMode /*, vos tables de teintes ici */)
        {
            if (wAppliedColor == 0)
                return ScaleColor(wBaseColor);

            return ScaleColor(wBaseColor);
        }
    }
}
