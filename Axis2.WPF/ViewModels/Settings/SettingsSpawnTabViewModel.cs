using Axis2.WPF.Mvvm;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Axis2.WPF.ViewModels.Settings
{
    public class SettingsSpawnTabViewModel : BindableBase
    {
        private bool _showNPCs;
        private System.Windows.Media.Color _spawnBGColor;

        public bool ShowNPCs
        {
            get => _showNPCs;
            set => SetProperty(ref _showNPCs, value);
        }

        public System.Windows.Media.Color SpawnBGColor
        {
            get => _spawnBGColor;
            set => SetProperty(ref _spawnBGColor, value);
        }

        [JsonIgnore]
        public ICommand SelectSpawnBGColorCommand { get; }
        [JsonIgnore]
        public ICommand ResetSpawnSettingsCommand { get; }

        public SettingsSpawnTabViewModel()
        {
            // Initialize properties with default values
            ShowNPCs = false;
            SpawnBGColor = System.Windows.Media.Colors.LightBlue; // Default color

            SelectSpawnBGColorCommand = new RelayCommand(SelectSpawnBGColor);
            ResetSpawnSettingsCommand = new RelayCommand(ResetSpawnSettings);
        }

        private void SelectSpawnBGColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SpawnBGColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            }
        }

        public void ResetSpawnSettings()
        {
            ShowNPCs = false;
            SpawnBGColor = System.Windows.Media.Colors.LightBlue;
        }
    }
}