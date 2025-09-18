using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Axis2.WPF.Models
{
    public class Category : INotifyPropertyChanged
    {
        private string _name = "";
        private ObservableCollection<SubCategory> _subSections = new();
        private bool _isExpanded; // New property

        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public ObservableCollection<SubCategory> SubSections { get => _subSections; set { _subSections = value; OnPropertyChanged(nameof(SubSections)); } }
        public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); } } // New public property

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
