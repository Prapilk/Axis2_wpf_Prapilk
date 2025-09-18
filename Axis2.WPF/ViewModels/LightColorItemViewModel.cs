using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using System.Collections.Generic;
using System.Windows.Media;

namespace Axis2.WPF.ViewModels
{
    public class LightColorItemViewModel : BindableBase
    {
        private ItemTileDataItem _itemTileDataItem;
        public ItemTileDataItem ItemTileDataItem
        {
            get => _itemTileDataItem;
            set
            {
                if (SetProperty(ref _itemTileDataItem, value))
                {
                    UpdateColors();
                }
            }
        }

        private LightMulItem _lightMulItem;
        public LightMulItem LightMulItem
        {
            get => _lightMulItem;
            set
            {
                if (SetProperty(ref _lightMulItem, value))
                {
                    UpdateColors();
                }
            }
        }

        public DrawConfigEntry? DrawConfigEntry { get; set; }

        public string? Comment { get; set; }

        public string Name => !string.IsNullOrEmpty(Comment) ? $"{Comment} (0x{ItemTileDataItem?.Id:X4})" : $"{ItemTileDataItem?.Name} (0x{ItemTileDataItem?.Id:X4})";

        public byte Layer => ItemTileDataItem?.Quality ?? 0;

        public List<System.Windows.Media.Color> Colors { get; private set; } = new List<System.Windows.Media.Color>();

        private void UpdateColors()
        {
            Colors.Clear();
            if (LightMulItem != null)
            {
                for (int i = 0; i < LightMulItem.ColorTable.Length; i += 2)
                {
                    ushort colorValue = (ushort)(LightMulItem.ColorTable[i] | (LightMulItem.ColorTable[i + 1] << 8));
                    byte r = (byte)((colorValue >> 10) & 0x1F);
                    byte g = (byte)((colorValue >> 5) & 0x1F);
                    byte b = (byte)(colorValue & 0x1F);
                    Colors.Add(System.Windows.Media.Color.FromRgb((byte)(r << 3), (byte)(g << 3), (byte)(b << 3)));
                }
            }
            OnPropertyChanged(nameof(Colors));
        }
    }
}