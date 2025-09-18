using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Axis2.WPF.Views
{
    public partial class LightWizardView : System.Windows.Controls.UserControl
    {
        public LightWizardView()
        {
            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var viewModel = DataContext as ViewModels.LightWizardViewModel;
            if (viewModel != null)
            {
                var searchText = (sender as System.Windows.Controls.TextBox).Text;
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search...")
                {
                    LightEffectsDataGrid.ItemsSource = viewModel.LightColorItems;
                }
                else
                {
                    var filteredItems = new System.Collections.ObjectModel.ObservableCollection<ViewModels.LightColorItemViewModel>(
                        viewModel.LightColorItems.Where(item =>
                            (item.Name != null && item.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)))
                    );
                    LightEffectsDataGrid.ItemsSource = filteredItems;
                }
            }
        }

        
    }
}
