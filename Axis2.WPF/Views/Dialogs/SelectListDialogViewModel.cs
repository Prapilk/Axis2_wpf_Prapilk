using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;

namespace Axis2.WPF.Views.Dialogs
{
    public class SelectListDialogViewModel : BindableBase
    {
        public string Title { get; }
        public string Message { get; }
        public ObservableCollection<string> Items { get; }

        private string? _selectedItem;
        public string? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool DialogResult { get; private set; }

        public SelectListDialogViewModel(string title, string message, ObservableCollection<string> items)
        {
            Title = title;
            Message = message;
            Items = items;

            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private bool CanOk()
        {
            return !string.IsNullOrEmpty(SelectedItem);
        }

        private void OnOk()
        {
            DialogResult = true;
        }

        private void OnCancel()
        {
            DialogResult = false;
        }
    }
}