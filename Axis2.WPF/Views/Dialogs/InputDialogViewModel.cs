using Axis2.WPF.Mvvm;
using System.Windows.Input;

namespace Axis2.WPF.Views.Dialogs
{
    public class InputDialogViewModel : BindableBase
    {
        private string _input = string.Empty;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public string Title { get; }
        public string Message { get; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool DialogResult { get; private set; }

        public InputDialogViewModel(string title, string message, string initialInput = "")
        {
            Title = title;
            Message = message;
            Input = initialInput;

            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
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