using Axis2.WPF.Mvvm;
using Axis2.WPF.ViewModels.Settings;
using Axis2.WPF.Services;
using Axis2.WPF.Models;
using System.Windows.Input;

namespace Axis2.WPF.ViewModels
{
    public class SettingsTabViewModel : BindableBase
    {
        private readonly SettingsService _settingsService;
        private AllSettings _allSettings;

        public SettingsGeneralViewModel SettingsGeneralViewModel { get; private set; }
        public SettingsFilePathsViewModel SettingsFilePathsViewModel { get; private set; }
        public SettingsItemTabViewModel SettingsItemTabViewModel { get; private set; }
        public SettingsTravelTabViewModel SettingsTravelTabViewModel { get; private set; }
        public SettingsSpawnTabViewModel SettingsSpawnTabViewModel { get; private set; }
        public SettingsOverridePathsViewModel SettingsOverridePathsViewModel { get; private set; }

        public ICommand ResetAllSettingsCommand { get; }

        public SettingsTabViewModel()
        {
            _settingsService = new SettingsService();
            _allSettings = _settingsService.LoadSettings();

            SettingsGeneralViewModel = _allSettings.GeneralSettings;
            SettingsGeneralViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
            SettingsFilePathsViewModel = _allSettings.FilePathsSettings;
            SettingsFilePathsViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
            SettingsItemTabViewModel = _allSettings.ItemTabSettings;
            SettingsItemTabViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
            SettingsTravelTabViewModel = _allSettings.TravelTabSettings;
            SettingsTravelTabViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
            SettingsSpawnTabViewModel = _allSettings.SpawnTabSettings;
            SettingsSpawnTabViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
            SettingsOverridePathsViewModel = _allSettings.OverridePathsSettings;
            SettingsOverridePathsViewModel.PropertyChanged += OnSubViewModelPropertyChanged;

            ResetAllSettingsCommand = new RelayCommand(ResetAllSettings);
        }

        private void OnSubViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is SettingsFilePathsViewModel filePathsViewModel && e.PropertyName == nameof(SettingsFilePathsViewModel.DefaultMulPath))
            {
                SettingsOverridePathsViewModel.UpdateDefaultPaths(filePathsViewModel.DefaultMulPath);
            }
            _settingsService.SaveSettings(_allSettings);
        }

        private void ResetAllSettings()
        {
            SettingsGeneralViewModel.ResetGeneralSettings();
            SettingsFilePathsViewModel.ResetPathsSettings();
            SettingsItemTabViewModel.ResetItemsSettings();
            SettingsTravelTabViewModel.ResetTravelSettings();
            SettingsSpawnTabViewModel.ResetSpawnSettings();
            SettingsOverridePathsViewModel.ResetAllPaths();

            _settingsService.SaveSettings(_allSettings);
        }
    }
}