using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Axis2.WPF.Services
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string message);
        Task<bool?> ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
        Task<string?> ShowInputDialog(string title, string message, string initialInput = "");
        Task<string?> ShowSelectListDialog(string title, string message, ObservableCollection<string> items);
    }
}