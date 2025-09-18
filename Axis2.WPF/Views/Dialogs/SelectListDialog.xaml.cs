using System.Windows;

namespace Axis2.WPF.Views.Dialogs
{
    public partial class SelectListDialog : Window
    {
        public SelectListDialog()
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