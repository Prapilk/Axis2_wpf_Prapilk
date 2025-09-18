using System;
using System.Windows;
using Axis2.WPF.Models; // Added for SObjectType
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel; // Added for ObservableCollection

namespace Axis2.WPF.Shared
{
    public class SearchField
    {
        public static SearchField Name { get; } = new SearchField("Name");
        public static SearchField Description { get; } = new SearchField("Description");
        public static SearchField ID { get; } = new SearchField("ID");
        public static SearchField Type { get; } = new SearchField("Type");

        public string DisplayName { get; private set; }

        private SearchField(string displayName)
        {
            DisplayName = displayName;
        }

        public static System.Collections.IEnumerable Values
        {
            get
            {
                yield return Name;
                yield return Description;
                yield return ID;
                yield return Type;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class SearchFieldToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchField searchField && parameter is string targetSearchFieldString)
            {
                return searchField.DisplayName == targetSearchFieldString ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SearchCriteria
    {
        public string SearchTerm { get; set; }
        public SearchField SelectedSearchField { get; set; }
        public bool IsLightSource { get; set; }
        public SObjectType? SelectedSObjectType { get; set; }
    }

    /// <summary>
    /// Interaction logic for FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public event Action<SearchCriteria>? SearchRequested;

        public ObservableCollection<string> AvailableScriptTypes { get; set; }

        public FindWindow(IEnumerable<string> scriptTypes)
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;
            Loaded += (sender, e) => SearchTermTextBox.Focus();

            // Set default selected item for SearchFieldComboBox
            SearchFieldComboBox.SelectedItem = SearchField.Description;

            AvailableScriptTypes = new ObservableCollection<string>(scriptTypes.OrderBy(s => s)); // Populate and sort
            // Set DataContext for XAML binding
            this.DataContext = this;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            SearchRequested?.Invoke(new SearchCriteria
            {
                SelectedSearchField = SearchFieldComboBox.SelectedItem as SearchField,
                IsLightSource = LightSourceCheckBox.IsChecked ?? false,
                SelectedSObjectType = null, // No longer used for Type search
                SearchTerm = (SearchFieldComboBox.SelectedItem as SearchField)?.DisplayName == "Type" ? SObjectTypeComboBox.SelectedItem as string : SearchTermTextBox.Text
            });
            this.Close();
        }
    }
}