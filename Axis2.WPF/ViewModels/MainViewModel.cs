using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using Axis2.WPF.ViewModels.Settings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.IO;

namespace Axis2.WPF.ViewModels
{
    public class MainViewModel : BindableBase, IHandler<ProfileLoadedEvent>
    {
        private readonly SettingsService _settingsService;
        private readonly ProfileService _profileService;
        private readonly EventAggregator _eventAggregator;
        private AllSettings _allSettings;

        // Services partagés
        private readonly BodyDefService _bodyDefService;
        private readonly TravelTabViewModel _travelTabViewModel;
        private readonly MulFileManager _mulFileManager;
        private readonly MobTypesService _mobTypesService;
        private readonly IUoArtService _uoArtService; // New field
        private readonly LightDataService _lightDataService; // New field

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public GeneralTabViewModel GeneralTabViewModel { get; }
        public ItemTabViewModel ItemTabViewModel { get; }
        public SettingsTabViewModel SettingsTabViewModel { get; }
        public ProfilesTabViewModel ProfilesTabViewModel { get; }
        public AccountTabViewModel AccountTabViewModel { get; }
        public CommandsTabViewModel CommandsTabViewModel { get; }
        public ItemTweakTabViewModel ItemTweakTabViewModel { get; }
        public LauncherTabViewModel LauncherTabViewModel { get; }
        public LogTabViewModel LogTabViewModel { get; }
        public MiscTabViewModel MiscTabViewModel { get; }
        public PlayerTweakTabViewModel PlayerTweakTabViewModel { get; }
        public ReminderTabViewModel ReminderTabViewModel { get; }
        public SpawnTabViewModel SpawnTabViewModel { get; }
        public TravelTabViewModel TravelTabViewModel { get; }

        private FileManager _fileManager; // Add FileManager field
        private AnimationManager _animationManager; // Add AnimationManager field

        public MainViewModel(
            FileManager fileManager, AnimationManager animationManager, BodyDefService bodyDefService,
            TravelTabViewModel travelTabViewModel, EventAggregator eventAggregator,
            SettingsService settingsService, ProfileService profileService, MulFileManager mulFileManager,
            MobTypesService mobTypesService, ScriptParser scriptParser, DialogService dialogService,
            UoClientCommunicator uoClientCommunicator, UoClient uoClient, IUoArtService uoArtService, LightDataService lightDataService,
            GeneralTabViewModel generalTabViewModel, ItemTabViewModel itemTabViewModel,
            SettingsTabViewModel settingsTabViewModel, ProfilesTabViewModel profilesTabViewModel,
            AccountTabViewModel accountTabViewModel, CommandsTabViewModel commandsTabViewModel,
            ItemTweakTabViewModel itemTweakTabViewModel, LauncherTabViewModel launcherTabViewModel,
            LogTabViewModel logTabViewModel, MiscTabViewModel miscTabViewModel,
            PlayerTweakTabViewModel playerTweakTabViewModel, ReminderTabViewModel reminderTabViewModel,
            SpawnTabViewModel spawnTabViewModel)
        {
            _fileManager = fileManager;
            _animationManager = animationManager;
            _bodyDefService = bodyDefService;
            _travelTabViewModel = travelTabViewModel;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _profileService = profileService;
            _mulFileManager = mulFileManager;
            _mobTypesService = mobTypesService;
            _uoArtService = uoArtService;
            _lightDataService = lightDataService;
            _eventAggregator.Subscribe(this);

            _allSettings = _settingsService.LoadSettings();

            GeneralTabViewModel = generalTabViewModel;
            ItemTabViewModel = new ItemTabViewModel(
                mulFileManager, scriptParser, eventAggregator, uoClient,
                settingsTabViewModel.SettingsItemTabViewModel, _lightDataService, (UoArtService)uoArtService, _settingsService);
            ItemTabViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ItemTabViewModel.SelectedItem))
                {
                    ItemTweakTabViewModel.SelectedItem = ItemTabViewModel.SelectedItem;
                }
            };
            SettingsTabViewModel = settingsTabViewModel;
            SettingsTabViewModel.SettingsGeneralViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SettingsGeneralViewModel.AlwaysOnTop))
                {
                    UpdateMainWindowTopmost();
                }
            };
            ProfilesTabViewModel = profilesTabViewModel;
            AccountTabViewModel = accountTabViewModel;
            CommandsTabViewModel = commandsTabViewModel;
            ItemTweakTabViewModel = itemTweakTabViewModel;
            LauncherTabViewModel = launcherTabViewModel;
            LogTabViewModel = logTabViewModel;
            MiscTabViewModel = miscTabViewModel;
            PlayerTweakTabViewModel = playerTweakTabViewModel;
            ReminderTabViewModel = reminderTabViewModel;
            SpawnTabViewModel = spawnTabViewModel;
            TravelTabViewModel = travelTabViewModel;

            ReloadServices();

            StatusMessage = "Ready. Please load a profile.";
        }

        private void ReloadServices()
        {
            _allSettings = _settingsService.LoadSettings();

            string baseMulPath = _allSettings.FilePathsSettings.DefaultMulPath;
            string bodyDefPath = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName == "body.def")?.FilePath ?? Path.Combine(baseMulPath, "body.def");
            string bodyConvPath = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName == "bodyconv.def")?.FilePath ?? Path.Combine(baseMulPath, "bodyconv.def");
            string mobTypesPath = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName == "mobtypes.txt")?.FilePath ?? Path.Combine(baseMulPath, "mobtypes.txt");

            _bodyDefService.Load(bodyDefPath, bodyConvPath);
            _mobTypesService.LoadMobTypes(mobTypesPath);

            // Get art and hues paths from FilePathsSettings
            string artMulPath = _allSettings.FilePathsSettings.ArtMul;
            string artIdxPath = _allSettings.FilePathsSettings.ArtIdx;
            string huesMulPath = _allSettings.FilePathsSettings.HuesMul;

            // Apply overrides if they exist
            var artMulOverride = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("art.mul", StringComparison.OrdinalIgnoreCase));
            var artIdxOverride = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("artidx.mul", StringComparison.OrdinalIgnoreCase));
            var huesMulOverride = _allSettings.OverridePathsSettings.FilePaths.FirstOrDefault(f => f.FileName.Equals("hues.mul", StringComparison.OrdinalIgnoreCase));

            if (artMulOverride != null && !string.IsNullOrEmpty(artMulOverride.FilePath)) artMulPath = artMulOverride.FilePath;
            if (artIdxOverride != null && !string.IsNullOrEmpty(artIdxOverride.FilePath)) artIdxPath = artIdxOverride.FilePath;
            if (huesMulOverride != null && !string.IsNullOrEmpty(huesMulOverride.FilePath)) huesMulPath = huesMulOverride.FilePath;

            _uoArtService.Load(_allSettings);

            // Appeler la nouvelle méthode Load de MulFileManager pour recharger les chemins
            _mulFileManager.Load(
                _allSettings.FilePathsSettings.ArtIdx,
                _allSettings.FilePathsSettings.ArtMul,
                _allSettings.FilePathsSettings.HuesMul,
                _allSettings.FilePathsSettings.AnimIdx,
                _allSettings.FilePathsSettings.AnimMul,
                _bodyDefService,
                _allSettings.OverridePathsSettings.FilePaths
            );
        }

        private void UpdateMainWindowTopmost()
        {
            if (System.Windows.Application.Current.MainWindow != null)
            {
                System.Windows.Application.Current.MainWindow.Topmost = SettingsTabViewModel.SettingsGeneralViewModel.AlwaysOnTop;
                Logger.Log($"DEBUG: MainWindow Topmost set to: {SettingsTabViewModel.SettingsGeneralViewModel.AlwaysOnTop}");
            }
        }

        public void Handle(ProfileLoadedEvent message)
        {
            System.Console.WriteLine($"DEBUG: MainViewModel - ProfileLoadedEvent received for profile: {message.LoadedProfile.Name}");
            ReloadServices();
        }

        private void LoadDefaultProfile(ObservableCollection<Profile> profiles)
        {
            var defaultProfile = profiles.FirstOrDefault(p => p.IsDefault);
            if (defaultProfile != null)
            {
                _eventAggregator.Publish(new ProfileLoadedEvent(defaultProfile));
            }
        }
    }
}