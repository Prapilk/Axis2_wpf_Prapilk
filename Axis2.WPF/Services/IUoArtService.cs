using System.Windows.Media.Imaging;

namespace Axis2.WPF.Services
{
    public interface IUoArtService
    {
        BitmapSource? GetItemArt(int itemID, int hue);
        BitmapSource? GetNpcArt(int npcID, int hue);
        System.Windows.Media.Color GetColorFromHue(ushort colorIndex);
        System.Windows.Media.Color GetColorFromHue(int hue, int shade);
        System.Windows.Media.Color GetColorFromDrawConfig(ushort drawConfigId);
        void Load(Models.AllSettings allSettings);
        BitmapSource? GetLightImage(ushort lightId, ushort colorId);
    }
}
