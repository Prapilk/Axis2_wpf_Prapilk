using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;

namespace Axis2.WPF.Models
{
    public class Profile : BindableBase
    {
        private string _name;
        private bool _isDefault;
        private bool _isWebProfile;
        private string _baseDirectory;
        private string _url;
        private bool _loadResource;
        private ObservableCollection<ScriptItem> _selectedScripts;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public bool IsWebProfile
        {
            get => _isWebProfile;
            set => SetProperty(ref _isWebProfile, value);
        }

        public string BaseDirectory
        {
            get => _baseDirectory;
            set => SetProperty(ref _baseDirectory, value);
        }

        public string URL
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public bool LoadResource
        {
            get => _loadResource;
            set => SetProperty(ref _loadResource, value);

        }

        public ObservableCollection<ScriptItem> SelectedScripts
        {
            get => _selectedScripts;
            set => SetProperty(ref _selectedScripts, value);
        }

        public Profile()
        {
            _name = string.Empty;
            _baseDirectory = string.Empty;
            _url = string.Empty;
            _selectedScripts = new ObservableCollection<ScriptItem>();
        }
    }
}