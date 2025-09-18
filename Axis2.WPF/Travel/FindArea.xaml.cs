using System.Windows;
using Axis2.WPF.ViewModels.Travel;

namespace Axis2.WPF.Travel
{
    public partial class FindArea : Window
    {
        public FindArea(FindAreaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}