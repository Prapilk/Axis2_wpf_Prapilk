using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.Json.Serialization;
using System.Windows.Forms; // For FolderBrowserDialog
using System.IO; // For Path.Combine

namespace Axis2.WPF.ViewModels.Settings
{
    public class SettingsGeneralViewModel : BindableBase
    {
        private bool _allowMultipleInstances;
        private bool _alwaysOnTop;
        private bool _sysClose;
        private bool _loadDefaultProfile;
        private bool _disableToolbar;
        private string _commandPrefix;
        private string _uoTitle;
        private string _selectedStartTab;
        
        public bool AllowMultipleInstances
        {
            get => _allowMultipleInstances;
            set => SetProperty(ref _allowMultipleInstances, value);
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetProperty(ref _alwaysOnTop, value);
        }

        public bool SysClose
        {
            get => _sysClose;
            set => SetProperty(ref _sysClose, value);
        }

        public bool LoadDefaultProfile
        {
            get => _loadDefaultProfile;
            set => SetProperty(ref _loadDefaultProfile, value);
        }

        public bool DisableToolbar
        {
            get => _disableToolbar;
            set => SetProperty(ref _disableToolbar, value);
        }

        public string CommandPrefix
        {
            get => _commandPrefix;
            set => SetProperty(ref _commandPrefix, value);
        }

        public string UOTitle
        {
            get => _uoTitle;
            set => SetProperty(ref _uoTitle, value);
        }

        public ObservableCollection<string> AvailableTabs { get; set; }

        public string SelectedStartTab
        {
            get => _selectedStartTab;
            set => SetProperty(ref _selectedStartTab, value);
        }

        [JsonIgnore]
        public ICommand ResetGeneralSettingsCommand { get; }
        

        public SettingsGeneralViewModel()
        {
            // Initialize properties with default values
            AllowMultipleInstances = false;
            AlwaysOnTop = false;
            SysClose = false;
            LoadDefaultProfile = false;
            DisableToolbar = false;
            CommandPrefix = ".";
            UOTitle = "Ultima Online";
            SelectedStartTab = "General";

            AvailableTabs = new ObservableCollection<string>
            {
                "General", "Account", "Commands", "Item", "Item Tweak", "Launcher", "Log", "Misc", "Player Tweak", "Reminder", "Spawn", "Travel", "Settings", "Profiles"
            };

            ResetGeneralSettingsCommand = new RelayCommand(ResetGeneralSettings);
            
        }

        public void ResetGeneralSettings()
        {
            AllowMultipleInstances = false;
            AlwaysOnTop = false;
            SysClose = false;
            LoadDefaultProfile = false;
            DisableToolbar = false;
            CommandPrefix = ".";
            UOTitle = "Ultima Online";
            SelectedStartTab = "General";
        }

        
    }
}