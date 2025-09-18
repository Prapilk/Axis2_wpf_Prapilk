using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axis2.WPF
{
    public static class WpfImageHelper
    {
        public static BitmapImage? ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapSource? CreateBitmapSource(byte[] pixelData, int width, int height, PixelFormat format)
        {
            if (pixelData == null || pixelData.Length == 0 || width == 0 || height == 0)
                return null;

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(
                width,
                height,
                96,
                96,
                format,
                null,
                pixelData,
                stride);
        }
    }
}