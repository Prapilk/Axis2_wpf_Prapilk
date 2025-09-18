using System.Windows;
using Axis2.WPF.Services;
using Axis2.WPF.ViewModels;
using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using System.Linq;

namespace Axis2.WPF
{
    public partial class App : System.Windows.Application
    {
        public MainViewModel MainViewModel { get; private set; }
        public static IUoArtService UoArtService { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logger.Init();

            // Services
            var settingsService = new SettingsService();
            var eventAggregator = new EventAggregator();
            var travelDataService = new TravelDataService();
            var locationService = new LocationService();
            var profileService = new ProfileService();
            var dialogService = new DialogService();
            var mobTypesService = new MobTypesService();
            var scriptParser = new ScriptParser();
            var scriptParserService = new ScriptParserService();
            var spellService = new SpellService(); // Instantiate SpellService
            var musicService = new MusicService(); // Instantiate MusicService
            var soundService = new SoundService(); // Instantiate SoundService

            var settings = settingsService.LoadSettings();
            var uoClientCommunicator = new UoClientCommunicator(settings.GeneralSettings.CommandPrefix, settings.GeneralSettings.UOTitle);
            var uoClient = new UoClient(uoClientCommunicator);

            // Correct order of declarations for dependencies
            var uopFilePaths = settings.OverridePathsSettings.FilePaths
                .Where(item => item.FileName.EndsWith(".uop", System.StringComparison.OrdinalIgnoreCase))
                .ToDictionary(item => item.FileName, item => item.FilePath, System.StringComparer.OrdinalIgnoreCase);

            Logger.Log("DEBUG: UOP File Paths:");
            foreach (var entry in uopFilePaths)
            {
                Logger.Log($"DEBUG:   {entry.Key} = {entry.Value}");
            }
            if (!uopFilePaths.Any())
            {
                Logger.Log("WARNING: No UOP files found in settings.OverridePathsSettings.FilePaths.");
            }

            var fileManager = new FileManager(uopFilePaths);
            var animationManager = new AnimationManager(fileManager);
            try
            {
                animationManager.LoadUOP();
            }
            catch (Exception ex)
            {
                Logger.Log($"ERROR: Failed to load UOP animations: {ex.Message}");
                // Optionally, re-throw or handle the exception more gracefully
            }
            var bodyDefService = new BodyDefService();
            var mulFileManager = new MulFileManager(fileManager, animationManager, bodyDefService);
            var uoArtService = new UoArtService(settings, mulFileManager); // Instancier UoArtService avec mulFileManager
            UoArtService = uoArtService;
            var mulMapService = new MulMapService(settingsService); // Added mulMapService declaration
            var lightDataService = new LightDataService(settings, uoArtService); // New LightDataService

            // ViewModels
            var travelTabViewModel = new TravelTabViewModel(travelDataService, mulMapService, settingsService, eventAggregator, locationService, uoClient, scriptParser, scriptParserService);
            var generalTabViewModel = new GeneralTabViewModel(uoClientCommunicator, dialogService);
            var settingsTabViewModel = new SettingsTabViewModel();
            var itemTabViewModel = new ItemTabViewModel(mulFileManager, scriptParser, eventAggregator, uoClient, settingsTabViewModel.SettingsItemTabViewModel, lightDataService, uoArtService, settingsService);
            var profiles = profileService.LoadProfiles();
            if (profiles.Count == 0)
            {
                profiles.Add(new Profile { Name = "<Axis Profile>", BaseDirectory = "C:\\Axis2\\Profiles\\Axis", IsDefault = true });
                profiles.Add(new Profile { Name = "<None>", BaseDirectory = "None" });
                profileService.SaveProfiles(profiles);
            }
            var profilesTabViewModel = new ProfilesTabViewModel(profileService, eventAggregator, profiles, null); // Will fix null later
            var accountTabViewModel = new AccountTabViewModel();
            var commandsTabViewModel = new CommandsTabViewModel();
            var itemTweakTabViewModel = new ItemTweakTabViewModel(uoClientCommunicator, scriptParser, dialogService, uoArtService, settings, eventAggregator, mulFileManager, bodyDefService, lightDataService);
            var launcherTabViewModel = new LauncherTabViewModel(settingsService);
            var logTabViewModel = new LogTabViewModel();
            var miscTabViewModel = new MiscTabViewModel(spellService, musicService, soundService, settingsService, eventAggregator, uoClient);
            var playerTweakTabViewModel = new PlayerTweakTabViewModel();
            var reminderTabViewModel = new ReminderTabViewModel();
            var spawnTabViewModel = new SpawnTabViewModel(mulFileManager, scriptParser, eventAggregator, uoClient, mobTypesService, animationManager, bodyDefService);

            MainViewModel = new MainViewModel(
                fileManager, animationManager, bodyDefService,
                travelTabViewModel, eventAggregator,
                settingsService, profileService, mulFileManager,
                mobTypesService, scriptParser, dialogService,
                uoClientCommunicator, uoClient, uoArtService, lightDataService,
                generalTabViewModel, itemTabViewModel,
                settingsTabViewModel, profilesTabViewModel,
                accountTabViewModel, commandsTabViewModel,
                itemTweakTabViewModel, launcherTabViewModel,
                logTabViewModel, miscTabViewModel,
                playerTweakTabViewModel, reminderTabViewModel,
                spawnTabViewModel);

            profilesTabViewModel.SetMainViewModel(MainViewModel);

            // Load default profile on startup
            var defaultProfile = profiles.FirstOrDefault(p => p.IsDefault);
            

            var mainWindow = new MainWindow
            {
                DataContext = MainViewModel
            };
            mainWindow.Show();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            if (MainViewModel != null && MainViewModel.ItemTweakTabViewModel != null)
            {
                if (!MainViewModel.ItemTweakTabViewModel.CheckAndSavePalette())
                {
                    // Cannot cancel exit from ExitEventArgs. Log for now.
                    //Logger.Log("Application exit cancelled by user (palette save).");
                }
            }
        }
    }
}

