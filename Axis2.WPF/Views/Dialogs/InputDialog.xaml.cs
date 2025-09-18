using System.Windows;

namespace Axis2.WPF.Views.Dialogs
{
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}