using Axis2.WPF.Mvvm;
using Axis2.WPF.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Axis2.WPF.Services;
using Microsoft.Win32;
using System.Linq;
using System;
using System.Windows.Interop;

namespace Axis2.WPF.ViewModels
{
    public class ProfilesTabViewModel : BindableBase
    {
        private readonly ProfileService _profileService;
        private readonly EventAggregator _eventAggregator;
        private MainViewModel _mainViewModel;

        private ObservableCollection<Profile> _profiles;
        private Profile? _selectedProfile;
        private ObservableCollection<ScriptItem> _availableScripts;

        public ObservableCollection<Profile> Profiles
        {
            get => _profiles;
            set => SetProperty(ref _profiles, value);
        }

        public Profile? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile != null)
                {
                    _selectedProfile.PropertyChanged -= SelectedProfile_PropertyChanged;
                }

                if (SetProperty(ref _selectedProfile, value))
                {
                    if (value != null)
                    {
                        value.PropertyChanged += SelectedProfile_PropertyChanged;
                    }
                    LoadAvailableScripts();
                }
            }
        }

        private void SelectedProfile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Profile changedProfile && e.PropertyName == nameof(Profile.IsDefault))
            {
                if (changedProfile.IsDefault)
                {
                    foreach (var profile in Profiles.Where(p => p != changedProfile))
                    {
                        profile.IsDefault = false;
                    }
                }
            }
        }

        public ObservableCollection<ScriptItem> AvailableScripts
        {
            get => _availableScripts;
            set => SetProperty(ref _availableScripts, value);
        }

        public ICommand AddProfileCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelProfileCommand { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand ExportProfileCommand { get; }
        public ICommand BrowseDirectoryCommand { get; }
        public ICommand SaveScriptsCommand { get; }

        public ProfilesTabViewModel(ProfileService profileService, EventAggregator eventAggregator, ObservableCollection<Profile> profiles, MainViewModel mainViewModel)
        {
            Logger.Log("DEBUG: ProfilesTabViewModel constructor entered.");
            _profileService = profileService;
            _eventAggregator = eventAggregator;
            _profiles = profiles;
            _mainViewModel = mainViewModel;
            _availableScripts = new ObservableCollection<ScriptItem>();

            AddProfileCommand = new RelayCommand(AddProfile);
            EditProfileCommand = new RelayCommand(EditProfile, CanEditOrDeleteProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile, CanEditOrDeleteProfile);
            SaveProfileCommand = new RelayCommand(SaveProfile);
            CancelProfileCommand = new RelayCommand(CancelProfile);
            LoadProfileCommand = new RelayCommand(LoadProfile, CanLoadProfile);
            ExportProfileCommand = new RelayCommand(ExportProfile, CanExportProfile);
            BrowseDirectoryCommand = new RelayCommand(BrowseDirectory);
            SaveScriptsCommand = new RelayCommand(SaveScripts);
        }

        // Method to set MainViewModel after it's fully constructed
        public void SetMainViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        private void AddProfile()
        {
            var newProfile = new Profile { Name = "New Profile" };
            Profiles.Add(newProfile);
            SelectedProfile = newProfile;
            _profileService.SaveProfiles(Profiles);
            _mainViewModel.StatusMessage = "New profile added and saved.";
        }

        private bool CanEditOrDeleteProfile()
        {
            return SelectedProfile != null && SelectedProfile.Name != "<Axis Profile>" && SelectedProfile.Name != "<None>";
        }

        private void EditProfile()
        {
            // Logic to enable editing of the selected profile
            // For now, properties are directly bindable, so no explicit 'edit mode' is needed.
        }

        private void DeleteProfile()
        {
            if (SelectedProfile != null)
            {
                Profiles.Remove(SelectedProfile);
                _profileService.SaveProfiles(Profiles);
                SelectedProfile = null;
            }
        }

        private void SaveProfile()
        {
            _profileService.SaveProfiles(Profiles);
        }

        private void CancelProfile()
        {
            // Revert changes by reloading profiles (simple approach for now)
            Profiles = _profileService.LoadProfiles();
        }

        private bool CanLoadProfile()
        {
            return SelectedProfile != null;
        }

        private void LoadProfile()
        {
            if (SelectedProfile == null) return;

            // First, save the current script selections to the profile object
            SaveScripts();

            if (!SelectedProfile.IsWebProfile && string.IsNullOrEmpty(SelectedProfile.BaseDirectory))
            {
                Logger.Log("Profile Error: The script directory for this local profile is not set. Please set it before loading.");
                return;
            }

            _mainViewModel.StatusMessage = $"Loading profile '{SelectedProfile.Name}'...";
            Logger.Log($"DEBUG: ProfilesTabViewModel - Publishing ProfileLoadedEvent for profile: {SelectedProfile.Name}");
            _eventAggregator.Publish(new ProfileLoadedEvent(SelectedProfile));
            _mainViewModel.StatusMessage = $"Profile '{SelectedProfile.Name}' loaded.";
        }

        private bool CanExportProfile()
        {
            return SelectedProfile != null && !SelectedProfile.IsWebProfile;
        }

        private void ExportProfile()
        {
            if (SelectedProfile == null || SelectedProfile.SelectedScripts == null || SelectedProfile.SelectedScripts.Count == 0)
            {
                Logger.Log("Export Profile: No scripts to export or no profile selected.");
                return;
            }

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Script Files (*.scp)|*.scp|All files (*.*)|*.*";
            saveFileDialog.FileName = SelectedProfile.Name + ".scp";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        foreach (var scriptItem in SelectedProfile.SelectedScripts)
                        {
                            writer.WriteLine(scriptItem.Path);
                        }
                    }
                    Logger.Log($"Scripts exported successfully to {saveFileDialog.FileName}");
                }
                catch (System.Exception ex)
                {
                    Logger.Log($"Error exporting scripts: {ex.Message}");
                }
            }
        }

        private void BrowseDirectory()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (SelectedProfile != null && !string.IsNullOrEmpty(SelectedProfile.BaseDirectory))
                {
                    dialog.SelectedPath = SelectedProfile.BaseDirectory;
                }

                System.Windows.Forms.DialogResult result = dialog.ShowDialog(new Wpf32Window(System.Windows.Application.Current.MainWindow));

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (SelectedProfile != null)
                    {
                        SelectedProfile.BaseDirectory = dialog.SelectedPath;
                        _profileService.SaveProfiles(Profiles); // Save changes after browsing
                    }
                }
            }
        }

        private void LoadAvailableScripts()
        {
            if (SelectedProfile == null || SelectedProfile.IsWebProfile || string.IsNullOrEmpty(SelectedProfile.BaseDirectory))
            {
                AvailableScripts = new ObservableCollection<ScriptItem>();
                return;
            }

            AvailableScripts = new ObservableCollection<ScriptItem>();
            var rootItem = new ScriptItem { Name = SelectedProfile.BaseDirectory, Path = SelectedProfile.BaseDirectory, IsFolder = true };
            AvailableScripts.Add(rootItem);

            ParseDirectory(SelectedProfile.BaseDirectory, rootItem, SelectedProfile.SelectedScripts);
        }

        private void ParseDirectory(string path, ScriptItem parentItem, ObservableCollection<ScriptItem> selectedScripts)
        {
            try
            {
                foreach (var directory in System.IO.Directory.GetDirectories(path))
                {
                    var dirInfo = new System.IO.DirectoryInfo(directory);
                    if (dirInfo.Name == "." || dirInfo.Name == "..") continue;

                    var folderItem = new ScriptItem { Name = dirInfo.Name, Path = directory, IsFolder = true };
                    parentItem.Children.Add(folderItem);
                    ParseDirectory(directory, folderItem, selectedScripts);
                }

                foreach (var file in System.IO.Directory.GetFiles(path))
                {
                    var fileInfo = new System.IO.FileInfo(file);
                    if (fileInfo.Extension.ToLower() == ".scp" || fileInfo.Name.ToLower() == "sphere.ini")
                    {
                        var scriptItem = new ScriptItem { Name = fileInfo.Name, Path = file, IsFolder = false };
                        scriptItem.IsSelected = selectedScripts.Any(s => s.Path == file);
                        parentItem.Children.Add(scriptItem);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log or handle the exception
                Logger.Log($"Error parsing directory {path}: {ex.Message}");
            }
        }

        private void GetSelections(ScriptItem item, ObservableCollection<ScriptItem> selections)
        {
            if (!item.IsFolder && item.IsSelected)
            {
                selections.Add(item);
            }

            foreach (var child in item.Children)
            {
                GetSelections(child, selections);
            }
        }

        private void SaveScripts()
        {
            Logger.Log("DEBUG: SaveScripts method entered.");
            if (SelectedProfile == null) return;

            SelectedProfile.SelectedScripts.Clear();
            foreach (var item in AvailableScripts)
            {
                GetSelections(item, SelectedProfile.SelectedScripts);
            }
            Logger.Log($"DEBUG: ProfilesTabViewModel - Scripts to save: {string.Join(", ", SelectedProfile.SelectedScripts.Select(s => s.Path))}");
            _profileService.SaveProfiles(Profiles); // Save all profiles after script selection changes
            Logger.Log($"DEBUG: ProfilesTabViewModel - Scripts saved for {SelectedProfile.Name}: {string.Join(", ", SelectedProfile.SelectedScripts.Select(s => s.Path))}");
        }
    }

    public class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(System.Windows.Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }
    }
}