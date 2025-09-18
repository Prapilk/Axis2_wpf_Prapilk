using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axis2.WPF.Services
{
    public class MapService
    {
        public WriteableBitmap? RenderMap(string mapPath, int width, int height)
        {
            if (!File.Exists(mapPath))
            {
                return null;
            }

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            using (var fileStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        binaryReader.BaseStream.Seek((y * width + x) * 3, SeekOrigin.Begin);
                        short tileId = binaryReader.ReadInt16();
                        sbyte z = (sbyte)binaryReader.ReadByte();

                        System.Windows.Media.Color color = GetColorForZ(z);
                        byte[] colorData = { color.B, color.G, color.R, 255 };
                        bitmap.WritePixels(new System.Windows.Int32Rect(x, y, 1, 1), colorData, 3, 0);
                    }
                }
            }

            return bitmap;
        }

        private System.Windows.Media.Color GetColorForZ(sbyte z)
        {
            if (z < -5)
                return Colors.DarkBlue; // Deep Water
            if (z < 0)
                return Colors.Blue; // Water
            if (z < 5)
                return Colors.LightGreen; // Grass
            if (z < 20)
                return Colors.Green; // Forest
            if (z < 40)
                return Colors.Gray; // Rock
            return Colors.White; // Snow
        }
    }
}