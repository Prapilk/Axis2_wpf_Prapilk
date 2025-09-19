using System.Windows;
using System.Threading.Tasks;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Views;
using Axis2.WPF.Views.Dialogs; // Added for InputDialog
using System.Collections.ObjectModel; // Added for ObservableCollection

namespace Axis2.WPF.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmation(string title, string message)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        public async Task<bool?> ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            if (viewModel is IDialog dialogViewModel)
            {
                Window window = new DialogWindow();
                System.Windows.Controls.UserControl? viewContent = null;

                if (viewModel is ViewModels.ColorSelectionViewModel)
                {
                    viewContent = new Views.ColorSelectionView();
                }
                else if (viewModel is ViewModels.LightWizardViewModel)
                {
                    viewContent = new Views.LightWizardView();
                }
                // Add more ViewModel-to-View mappings here as needed
                // else if (viewModel is OtherViewModel)
                // {
                //     viewContent = new OtherView();
                // }

                if (viewContent == null)
                {
                    // Fallback if no specific view is found, or throw an exception
                    throw new InvalidOperationException($"No view found for ViewModel type {typeof(TViewModel).Name}");
                }

                window.Content = viewContent;
                window.DataContext = viewModel;
                window.Title = dialogViewModel.Title;
                window.Owner = System.Windows.Application.Current.MainWindow;

                dialogViewModel.CloseRequested += (sender, args) =>
                {
                    window.DialogResult = dialogViewModel.DialogResult;
                    window.Close();
                };

                return window.ShowDialog();
            }
            return false;
        }

        public async Task<string?> ShowInputDialog(string title, string message, string initialInput = "")
        {
            var viewModel = new InputDialogViewModel(title, message, initialInput);
            var dialog = new InputDialog { DataContext = viewModel };

            // Set the owner to the current active window if possible
            if (System.Windows.Application.Current.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            bool? result = dialog.ShowDialog();

            if (result == true && viewModel.DialogResult == true)
            {
                return viewModel.Input;
            }
            return null;
        }

        public async Task<string?> ShowSelectListDialog(string title, string message, ObservableCollection<string> items)
        {
            var viewModel = new SelectListDialogViewModel(title, message, items);
            var dialog = new SelectListDialog { DataContext = viewModel };

            // Set the owner to the current active window if possible
            if (System.Windows.Application.Current.MainWindow != null)
            {
                dialog.Owner = System.Windows.Application.Current.MainWindow;
            }

            bool? result = dialog.ShowDialog();

            if (result == true && viewModel.DialogResult == true)
            {
                return viewModel.SelectedItem;
            }
            return null;
        }
    }
}