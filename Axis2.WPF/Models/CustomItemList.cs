using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;

namespace Axis2.WPF.Models
{
    public class CustomItemList : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<SObject> Items { get; set; } = new ObservableCollection<SObject>();
    }
}
