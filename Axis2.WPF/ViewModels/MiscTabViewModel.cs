using Axis2.WPF.Mvvm;
using Axis2.WPF.Models;
using Axis2.WPF.Services;
using System.Collections.ObjectModel;
using System.IO; // For File.ReadAllText
using System.Linq; // For LINQ
using System.Threading.Tasks; // For Task.Run
using System.Windows.Input; // For ICommand
using System.Windows.Media; // For MediaPlayer
using System.Media; // For SoundPlayer

namespace Axis2.WPF.ViewModels
{
    public class MiscTabViewModel : ViewModelBase, IHandler<ProfileLoadedEvent>
    {
        private readonly SpellService _spellService;
        private readonly MusicService _musicService;
        private readonly SoundService _soundService;
        private readonly SettingsService _settingsService;
        private readonly EventAggregator _eventAggregator;
        private readonly IUoClient _uoClient;

        private MediaPlayer _mediaPlayer;
        private SoundPlayer _soundPlayer;

        public ObservableCollection<Spell> Spells { get; } = new ObservableCollection<Spell>();
        public ObservableCollection<MusicTrack> MusicTracks { get; } = new ObservableCollection<MusicTrack>();
        public ObservableCollection<Sound> Sounds { get; } = new ObservableCollection<Sound>();

        private MusicTrack _selectedMusicTrack;
        public MusicTrack SelectedMusicTrack
        {
            get => _selectedMusicTrack;
            set => SetProperty(ref _selectedMusicTrack, value);
        }

        private Sound _selectedSound;
        public Sound SelectedSound
        {
            get => _selectedSound;
            set => SetProperty(ref _selectedSound, value);
        }

        private Spell _selectedSpell;
        public Spell SelectedSpell
        {
            get => _selectedSpell;
            set => SetProperty(ref _selectedSpell, value);
        }

        public ICommand PlayMusicCommand { get; }
        public ICommand StopMusicCommand { get; }
        public ICommand SendMusicCommand { get; }

        public ICommand PlaySoundCommand { get; }
        public ICommand StopSoundCommand { get; }
        public ICommand SendSoundCommand { get; }

        public ICommand CastSpellCommand { get; }

        public MiscTabViewModel(SpellService spellService, MusicService musicService, SoundService soundService, SettingsService settingsService, EventAggregator eventAggregator, IUoClient uoClient)
        {
            _spellService = spellService;
            _musicService = musicService;
            _soundService = soundService;
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;
            _uoClient = uoClient;

            _mediaPlayer = new MediaPlayer();
            _soundPlayer = new SoundPlayer();

            PlayMusicCommand = new RelayCommand(PlayMusic, CanPlayMusic);
            StopMusicCommand = new RelayCommand(StopMusic, CanStopMusic);
            SendMusicCommand = new RelayCommand(SendMusic, CanSendMusic);

            PlaySoundCommand = new RelayCommand(PlaySound, CanPlaySound);
            StopSoundCommand = new RelayCommand(StopSound, CanStopSound);
            SendSoundCommand = new RelayCommand(SendSound, CanSendSound);

            CastSpellCommand = new RelayCommand(CastSpell, CanCastSpell);

            _eventAggregator.Subscribe(this);
        }

        public void Handle(ProfileLoadedEvent e)
        {
            if (e?.LoadedProfile == null)
            {
                Logger.Log("ERROR: MiscTabViewModel: OnProfileLoaded received a null profile.");
                return;
            }
            Logger.Log($"DEBUG: MiscTabViewModel: Profile name: {e.LoadedProfile.Name}");

            LoadSpells(e.LoadedProfile);
            LoadMusic(e.LoadedProfile);
            LoadSounds(e.LoadedProfile);
        }

        private async void LoadSpells(Profile profile)
        {
            Spells.Clear();
            if (profile == null || !profile.SelectedScripts.Any())
            {
                Logger.Log("MiscTabViewModel: Profile or SelectedScripts are null or empty. Clearing spells.");
                return;
            }

            var spellScriptPaths = profile.SelectedScripts.Select(s => s.Path).Distinct().ToList();
            Logger.Log($"DEBUG: MiscTabViewModel - Found {spellScriptPaths.Count} unique scripts to parse for spells.");

            foreach (var scriptPath in spellScriptPaths)
            {
                if (File.Exists(scriptPath))
                {
                    string scriptContent = await Task.Run(() => File.ReadAllText(scriptPath));
                    var parsedSpells = _spellService.ParseSpells(scriptContent);
                    foreach (var spell in parsedSpells)
                    {
                        Spells.Add(spell);
                    }
                }
                else
                {
                    Logger.Log($"WARNING: MiscTabViewModel - Script file not found: {scriptPath}");
                }
            }
            //Logger.Log($"DEBUG: MiscTabViewModel - Loaded {Spells.Count} spells.");
        }

        private void LoadMusic(Profile profile)
        {
            MusicTracks.Clear();
            // Get the DefaultMulPath from settings
            var settings = _settingsService.LoadSettings();
            string defaultMulPath = settings.FilePathsSettings.DefaultMulPath;

            if (string.IsNullOrEmpty(defaultMulPath))
            {
                Logger.Log("WARNING: DefaultMulPath is not set. Cannot load music.");
                return;
            }

            string musicDirectory = Path.Combine(defaultMulPath, "Music", "Digital");
            Logger.Log($"DEBUG: Attempting to load music from: {musicDirectory}");

            var loadedTracks = _musicService.LoadMusicTracks(musicDirectory);
            foreach (var track in loadedTracks)
            {
                MusicTracks.Add(track);
            }
            Logger.Log($"DEBUG: MiscTabViewModel - Loaded {MusicTracks.Count} music tracks.");
        }

        private void LoadSounds(Profile profile)
        {
            Sounds.Clear();
            var settings = _settingsService.LoadSettings();
            string defaultMulPath = settings.FilePathsSettings.DefaultMulPath;

            if (string.IsNullOrEmpty(defaultMulPath))
            {
                Logger.Log("WARNING: DefaultMulPath is not set. Cannot load sounds.");
                return;
            }

            string soundidxPath = Path.Combine(defaultMulPath, "soundidx.mul");
            string soundmulPath = Path.Combine(defaultMulPath, "sound.mul");

            Logger.Log($"DEBUG: Attempting to load sounds from: {soundidxPath} and {soundmulPath}");

            var loadedSounds = _soundService.LoadSoundIndexes(soundidxPath, soundmulPath);
            foreach (var sound in loadedSounds)
            {
                Sounds.Add(sound);
            }
            Logger.Log($"DEBUG: MiscTabViewModel - Loaded {Sounds.Count} sounds.");
        }

        private bool CanPlayMusic() => SelectedMusicTrack != null && File.Exists(SelectedMusicTrack.FilePath);
        private void PlayMusic()
        {
            if (_mediaPlayer.Source != null && _mediaPlayer.Source.LocalPath == SelectedMusicTrack.FilePath)
            {
                _mediaPlayer.Play();
            }
            else
            {
                _mediaPlayer.Open(new Uri(SelectedMusicTrack.FilePath));
                _mediaPlayer.Play();
            }
            Logger.Log($"DEBUG: Playing music: {SelectedMusicTrack.Name}");
        }

        private bool CanStopMusic() => _mediaPlayer.Source != null;
        private void StopMusic()
        {
            _mediaPlayer.Stop();
            Logger.Log($"DEBUG: Stopped music.");
        }

        private bool CanSendMusic() => SelectedMusicTrack != null;
        private void SendMusic()
        {
            string command = $"music {SelectedMusicTrack.ID}"; // Changed to use ID
            _uoClient.SendToClient(command);
            Logger.Log($"DEBUG: Sent music command to client: {command}");
        }

        private bool CanPlaySound() => SelectedSound != null && SelectedSound.Length > 0;
        private void PlaySound()
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                string soundmulPath = Path.Combine(settings.FilePathsSettings.DefaultMulPath, "sound.mul");

                byte[] rawSoundData = _soundService.GetSoundData(soundmulPath, SelectedSound.StartOffset, SelectedSound.Length);
                if (rawSoundData != null)
                {
                    byte[] wavHeader = _soundService.CreateWavHeader(rawSoundData.Length);
                    byte[] fullWavData = new byte[wavHeader.Length + rawSoundData.Length];
                    Buffer.BlockCopy(wavHeader, 0, fullWavData, 0, wavHeader.Length);
                    Buffer.BlockCopy(rawSoundData, 0, fullWavData, wavHeader.Length, rawSoundData.Length);

                    using (MemoryStream ms = new MemoryStream(fullWavData))
                    {
                        _soundPlayer.Stream = ms;
                        _soundPlayer.Play();
                    }
                    //Logger.Log($"DEBUG: Playing sound: {SelectedSound.Name} (ID: {SelectedSound.ID})");
                }
                else
                {
                    Logger.Log($"WARNING: Could not get sound data for ID: {SelectedSound.ID}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ERROR: Failed to play sound: {ex.Message}");
            }
        }

        private bool CanStopSound() => _soundPlayer.Stream != null; // Can only stop if a sound is loaded
        private void StopSound()
        {
            _soundPlayer.Stop();
            Logger.Log($"DEBUG: Stopped sound.");
        }

        private bool CanSendSound() => SelectedSound != null;
        private void SendSound()
        {
            string command = $"sfx {SelectedSound.DisplayID}"; // Changed to use DisplayID
            _uoClient.SendToClient(command);
            Logger.Log($"DEBUG: Sent sound command to client: {command}");
        }

        private bool CanCastSpell() => SelectedSpell != null && !string.IsNullOrEmpty(SelectedSpell.DefName);
        private void CastSpell()
        {
            string command = $"cast {SelectedSpell.DefName}";
            _uoClient.SendToClient(command);
            Logger.Log($"DEBUG: Sent cast spell command to client: {command}");
        }
    }
}