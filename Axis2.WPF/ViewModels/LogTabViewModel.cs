using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;

namespace Axis2.WPF.ViewModels
{
    public class LogTabViewModel : ViewModelBase
    {
        public ObservableCollection<string> LogMessages { get; }

        public LogTabViewModel()
        {
            LogMessages = new ObservableCollection<string>();
            Logger.OnLogMessage += (message) =>
            {
                // Ensure the update is on the UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() => LogMessages.Add(message));
            };
        }
    }
}