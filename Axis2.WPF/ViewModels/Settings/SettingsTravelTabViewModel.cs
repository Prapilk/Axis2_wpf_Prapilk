using Axis2.WPF.Mvvm;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Axis2.WPF.ViewModels.Settings
{
    public class SettingsTravelTabViewModel : BindableBase
    {
        private bool _drawStatics;
        private bool _drawDifs;
        private bool _showSpawnpoints;
        private bool _showMap;
        private System.Windows.Media.Color _npcSpawnColor;
        private System.Windows.Media.Color _itemSpawnColor;

        public bool DrawStatics
        {
            get => _drawStatics;
            set => SetProperty(ref _drawStatics, value);
        }

        public bool DrawDifs
        {
            get => _drawDifs;
            set => SetProperty(ref _drawDifs, value);
        }

        public bool ShowSpawnpoints
        {
            get => _showSpawnpoints;
            set => SetProperty(ref _showSpawnpoints, value);
        }

        public bool ShowMap
        {
            get => _showMap;
            set
            {
                if (SetProperty(ref _showMap, value))
                {
                    // Update enablement of related controls
                    OnPropertyChanged(nameof(DrawStatics));
                    OnPropertyChanged(nameof(DrawDifs));
                    OnPropertyChanged(nameof(ShowSpawnpoints));
                    OnPropertyChanged(nameof(NPCSpawnColor));
                    OnPropertyChanged(nameof(ItemSpawnColor));
                }
            }
        }

        public System.Windows.Media.Color NPCSpawnColor
        {
            get => _npcSpawnColor;
            set => SetProperty(ref _npcSpawnColor, value);
        }

        public System.Windows.Media.Color ItemSpawnColor
        {
            get => _itemSpawnColor;
            set => SetProperty(ref _itemSpawnColor, value);
        }

        [JsonIgnore]
        public ICommand SelectNPCSpawnColorCommand { get; }
        [JsonIgnore]
        public ICommand SelectItemSpawnColorCommand { get; }
        [JsonIgnore]
        public ICommand ResetTravelSettingsCommand { get; }

        public SettingsTravelTabViewModel()
        {
            // Initialize properties with default values
            DrawStatics = false;
            DrawDifs = false;
            ShowSpawnpoints = false;
            ShowMap = false;
            NPCSpawnColor = System.Windows.Media.Colors.Red; // Default color
            ItemSpawnColor = System.Windows.Media.Colors.Blue; // Default color

            SelectNPCSpawnColorCommand = new RelayCommand(SelectNPCSpawnColor);
            SelectItemSpawnColorCommand = new RelayCommand(SelectItemSpawnColor);
            ResetTravelSettingsCommand = new RelayCommand(ResetTravelSettings);
        }

        private void SelectNPCSpawnColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                NPCSpawnColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
        }

        private void SelectItemSpawnColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                ItemSpawnColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
        }

        public void ResetTravelSettings()
        {
            DrawStatics = false;
            DrawDifs = false;
            ShowSpawnpoints = false;
            ShowMap = false;
            NPCSpawnColor = System.Windows.Media.Colors.Red;
            ItemSpawnColor = System.Windows.Media.Colors.Blue;
        }
    }
}