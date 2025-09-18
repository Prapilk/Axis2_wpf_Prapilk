using System.Windows;

namespace Axis2.WPF.Views.Travel
{
    public partial class AddEditRoomWindow : Window
    {
        public AddEditRoomWindow(ViewModels.Travel.AddEditRoomViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}