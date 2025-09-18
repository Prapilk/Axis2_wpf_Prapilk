using Axis2.WPF.Mvvm;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Axis2.WPF.ViewModels.Settings
{
    public class SettingsItemTabViewModel : BindableBase
    {
        private bool _roomView;
        private bool _showItems;
        private System.Windows.Media.Color _itemBGColor;

        public bool RoomView
        {
            get => _roomView;
            set => SetProperty(ref _roomView, value);
        }

        public bool ShowItems
        {
            get => _showItems;
            set => SetProperty(ref _showItems, value);
        }

        public System.Windows.Media.Color ItemBGColor
        {
            get => _itemBGColor;
            set => SetProperty(ref _itemBGColor, value);
        }

        [JsonIgnore]
        public ICommand SelectItemBGColorCommand { get; }
        [JsonIgnore]
        public ICommand ResetItemsSettingsCommand { get; }

        public SettingsItemTabViewModel()
        {
            // Initialize properties with default values
            RoomView = false;
            ShowItems = false;
            ItemBGColor = System.Windows.Media.Colors.LightGray; // Default color

            SelectItemBGColorCommand = new RelayCommand(SelectItemBGColor);
            ResetItemsSettingsCommand = new RelayCommand(ResetItemsSettings);
        }

        private void SelectItemBGColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                ItemBGColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
        }

        public void ResetItemsSettings()
        {
            RoomView = false;
            ShowItems = false;
            ItemBGColor = System.Windows.Media.Colors.LightGray;
        }
    }
}