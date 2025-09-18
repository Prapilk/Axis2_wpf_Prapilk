using System.Windows;

namespace Axis2.WPF.Views.Travel
{
    /// <summary>
    /// Interaction logic for AddEditRectWindow.xaml
    /// </summary>
    public partial class AddEditRectWindow : Window
    {
        public AddEditRectWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}