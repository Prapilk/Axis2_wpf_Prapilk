using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Axis2.WPF.ViewModels.Travel
{
    public class WorldMapViewModel : ViewModelBase
    {
        private readonly MulMapService _mapService;
        private readonly SettingsService _settingsService;
        private WriteableBitmap? _mapImage;

        public WorldMapViewModel(MulMapService mapService, SettingsService settingsService)
        {
            _mapService = mapService;
            _settingsService = settingsService;
            LoadMap();
        }

        public WriteableBitmap? MapImage
        {
            get => _mapImage;
            set => SetProperty(ref _mapImage, value);
        }

        private void LoadMap()
        {
            var settings = _settingsService.LoadSettings();
            var mapPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "map0.mul")?.FilePath;
            Logger.Log($"WorldMapViewModel: Path for map0.mul: {mapPath ?? "NOT FOUND"}");

            // Create the statics service for map 0
            var staidxPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "staidx0.mul")?.FilePath;
            Logger.Log($"WorldMapViewModel: Path for staidx0.mul: {staidxPath ?? "NOT FOUND"}");
            var staticsPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "statics0.mul")?.FilePath;
            Logger.Log($"WorldMapViewModel: Path for statics0.mul: {staticsPath ?? "NOT FOUND"}");
            var radarcolPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "radarcol.mul")?.FilePath;
            Logger.Log($"WorldMapViewModel: Path for radarcol.mul: {radarcolPath ?? "NOT FOUND"}");

            StaticsService? staticsService = null;
            if (!string.IsNullOrEmpty(staidxPath) && !string.IsNullOrEmpty(staticsPath) && !string.IsNullOrEmpty(radarcolPath))
            {
                staticsService = new StaticsService(staidxPath, staticsPath, radarcolPath);
            }

            if (!string.IsNullOrEmpty(mapPath) && staticsService != null)
            {
                Logger.Log("WorldMapViewModel: All paths found and StaticsService created. Attempting to render map.");
            {
                // To show the whole map, we need to adjust the zoom level.
                // Full map size is 6144x4096.
                // A negative zoom level zooms out. Let's use -3 to fit the map.
                int zoomLevel = -3;
                int centerX = 3072; // Center of the map (6144 / 2)
                int centerY = 2048; // Center of the map (4096 / 2)
                int viewPortWidth = 800; // Assuming window width
                int viewPortHeight = 600; // Assuming window heightj'ai 
                MapImage = _mapService.RenderMap(0, mapPath, viewPortWidth, viewPortHeight, zoomLevel, centerX, centerY, staticsService);
            }
        }
    }
    }
}