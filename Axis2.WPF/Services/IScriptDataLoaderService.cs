using System.Collections.ObjectModel;
using Axis2.WPF.Models;

namespace Axis2.WPF.Services
{
    public interface IScriptDataLoaderService
    {
        void LoadScripts(string scriptPath);
        ObservableCollection<CCategory> LoadItemCategories();
    }
}