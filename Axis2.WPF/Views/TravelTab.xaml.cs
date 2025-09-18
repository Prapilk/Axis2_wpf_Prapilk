using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Axis2.WPF.Services;
using System.Collections.Generic;
using System;
using System.IO;

namespace Axis2.WPF.Views
{
    /// <summary>
    /// Interaction logic for TravelTab.xaml
    /// </summary>
    public partial class TravelTab : System.Windows.Controls.UserControl
    {
        private const int MAP_WIDTH = 400;
        private const int MAP_HEIGHT = 400;

        public TravelTab()
        {
            InitializeComponent();
            this.Loaded += TravelTab_Loaded;
        }

        private void TravelTab_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set the DataContext here if it's not already set in XAML or elsewhere
            // For example, if you're instantiating the ViewModel in code:
            // this.DataContext = new TravelTabViewModel(...);
        }

        private void RegionsTreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.DataContext is ViewModels.TravelTabViewModel viewModel)
            {
                viewModel.SelectedRegion = e.NewValue as Axis2.WPF.Models.MapRegion;
            }
        }

        // The following methods are now handled by data binding in the ViewModel
        // and are kept here as placeholders or if specific UI-only logic is needed.
        private void ZoomIn_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void ZoomOut_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void WorldMap_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void Locate_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void Track_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void Go_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void Send_Click(object sender, System.Windows.RoutedEventArgs e) { }
        private void Where_Click(object sender, System.Windows.RoutedEventArgs e) { }
    }
}