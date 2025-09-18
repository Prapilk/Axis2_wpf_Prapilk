using System.Windows;
using Axis2.WPF.Services;
using Axis2.WPF.ViewModels.Travel;
using System.Linq;

namespace Axis2.WPF.Travel
{
    /// <summary>
    /// Interaction logic for WorldMapView.xaml
    /// </summary>
    public partial class WorldMapView : Window
    {
        public WorldMapView()
        {
            InitializeComponent();
            var settingsService = new SettingsService();
            var mapService = new MulMapService(settingsService);
            DataContext = new WorldMapViewModel(mapService, settingsService);
        }
    }
}
