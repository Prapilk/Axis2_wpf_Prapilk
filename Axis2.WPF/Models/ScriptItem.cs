using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;

namespace Axis2.WPF.Models
{
    public class ScriptItem : BindableBase
    {
        private string _name;
        private string _path;
        private bool _isFolder;
        private bool _isSelected;
        private ObservableCollection<ScriptItem> _children;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public bool IsFolder
        {
            get => _isFolder;
            set => SetProperty(ref _isFolder, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ObservableCollection<ScriptItem> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public ScriptItem()
        {
            _name = string.Empty;
            _path = string.Empty;
            _children = new ObservableCollection<ScriptItem>();
        }
    }
}