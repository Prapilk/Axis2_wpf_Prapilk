using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Axis2.WPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for LoadCustomListDialog.xaml
    /// </summary>
    public partial class LoadCustomListDialog : Window
    {
        public string SelectedListName { get; private set; }

        public LoadCustomListDialog()
        {
            InitializeComponent();
            PopulateCustomLists();
        }

        private void PopulateCustomLists()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var jsonFiles = Directory.GetFiles(appDirectory, "*.json")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .Where(name => name != "items") // Exclude the default items.json
                                         .ToList();
                CustomListsListView.ItemsSource = jsonFiles;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error populating custom lists: {ex.Message}", "Load Custom List Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CustomListsListView.SelectedItem is string selectedName)
            {
                SelectedListName = selectedName;
                DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a list to load.", "Load Custom List", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}