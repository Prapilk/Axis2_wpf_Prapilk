using Axis2.WPF.Models;
using Axis2.WPF.Shared;
using Axis2.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Axis2.WPF.Views
{
    public partial class SpawnTabView : System.Windows.Controls.UserControl
    {
        public SpawnTabView()
        {
            InitializeComponent();
        }

        // This is the original handler that makes the main TreeView work.
        // It finds the clicked item and sets it in the ViewModel.
        private void OnTreeViewItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && DataContext is SpawnTabViewModel viewModel)
            {
                if (treeViewItem.DataContext is Category || treeViewItem.DataContext is SubCategory)
                {
                    viewModel.SelectedTreeItem = treeViewItem.DataContext;
                }
            }
        }

        // This is the new handler for the third ListView (spawn group content).
        private void SpawnGroupMembersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SpawnTabViewModel viewModel && e.AddedItems.Count > 0 && e.AddedItems[0] is SpawnGroupMemberViewModel selectedMemberViewModel)
            {
                viewModel.SelectedMember = selectedMemberViewModel;
            }
        }

        // This is the handler for the Find button you said I deleted.
        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpawnTabViewModel viewModel)
            {
                var findWindow = new FindWindow(viewModel.GetUniqueScriptTypes());
                findWindow.Owner = System.Windows.Application.Current.MainWindow;
                findWindow.SearchRequested += FindWindow_SearchRequested;
                findWindow.ShowDialog();
            }
        }

        private void FindWindow_SearchRequested(Axis2.WPF.Shared.SearchCriteria searchCriteria)
        {
            if (DataContext is SpawnTabViewModel viewModel)
            {
                viewModel.FilterItems(searchCriteria);
            }
        }

    }
}
