using System.Windows;

namespace Axis2.WPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SaveCustomListDialog.xaml
    /// </summary>
    public partial class SaveCustomListDialog : Window
    {
        public string ListName { get; private set; }

        public SaveCustomListDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ListName = ListNameTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}