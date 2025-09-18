using Axis2.WPF.Mvvm;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System;
using Axis2.WPF.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Axis2.WPF.Models;
using System.Windows.Media.Imaging;
using Axis2.WPF.Extensions;
using System.Globalization;
using System.Text.Json;
using System.IO;
using Axis2.WPF.ViewModels;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Axis2.WPF.ViewModels
{
    public class ItemTweakTabViewModel : BindableBase, IHandler<SObjectSelectedEvent>, IHandler<ProfileLoadedEvent>
    {
        private readonly UoClientCommunicator _uoClientCommunicator;
        private readonly ScriptParser _scriptParser;
        private readonly IDialogService _dialogService;
        private readonly IUoArtService _uoArtService;
        private readonly AllSettings _settings;
        private readonly EventAggregator _eventAggregator;
        private readonly MulFileManager _mulFileManager;
        private readonly BodyDefService _bodyDefService;
        private readonly LightDataService _lightDataService; // New field
        private bool _isPaletteModified = false;

        private BitmapSource? _itemPreviewImage;
        public BitmapSource? ItemPreviewImage
        {
            get => _itemPreviewImage;
            set => SetProperty(ref _itemPreviewImage, value);
        }

        private SObject? _selectedItem;
        public SObject? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdatePreviewFromSelectedItem(value);
                }
            }
        }

        #region Attributes Properties
        private bool _isIdentified;
        public bool IsIdentified
        {
            get => _isIdentified;
            set
            {
                if (SetProperty(ref _isIdentified, value))
                {
                    UpdateAttributesValue(Constants.ATTR_IDENTIFIED, value);
                }
            }
        }

        private bool _isDecay;
        public bool IsDecay
        {
            get => _isDecay;
            set
            {
                if (SetProperty(ref _isDecay, value))
                {
                    UpdateAttributesValue(Constants.ATTR_DECAY, value);
                }
            }
        }

        private bool _isNewbie;
        public bool IsNewbie
        {
            get => _isNewbie;
            set
            {
                if (SetProperty(ref _isNewbie, value))
                {
                    UpdateAttributesValue(Constants.ATTR_NEWBIE, value);
                }
            }
        }

        private bool _isAlwaysMoveable;
        public bool IsAlwaysMoveable
        {
            get => _isAlwaysMoveable;
            set
            {
                if (SetProperty(ref _isAlwaysMoveable, value))
                {
                    UpdateAttributesValue(Constants.ATTR_MOVE_ALWAYS, value);
                }
            }
        }

        private bool _isNeverMoveable;
        public bool IsNeverMoveable
        {
            get => _isNeverMoveable;
            set
            {
                if (SetProperty(ref _isNeverMoveable, value))
                {
                    UpdateAttributesValue(Constants.ATTR_MOVE_NEVER, value);
                }
            }
        }

        private bool _isMagic;
        public bool IsMagic
        {
            get => _isMagic;
            set
            {
                if (SetProperty(ref _isMagic, value))
                {
                    UpdateAttributesValue(Constants.ATTR_MAGIC, value);
                }
            }
        }

        private bool _isOwnedByTown;
        public bool IsOwnedByTown
        {
            get => _isOwnedByTown;
            set
            {
                if (SetProperty(ref _isOwnedByTown, value))
                {
                    UpdateAttributesValue(Constants.ATTR_OWNED, value);
                }
            }
        }

        private bool _isInvisible;
        public bool IsInvisible
        {
            get => _isInvisible;
            set
            {
                if (SetProperty(ref _isInvisible, value))
                {
                    UpdateAttributesValue(Constants.ATTR_INVIS, value);
                }
            }
        }

        private bool _isCursed;
        public bool IsCursed
        {
            get => _isCursed;
            set
            {
                if (SetProperty(ref _isCursed, value))
                {
                    UpdateAttributesValue(Constants.ATTR_CURSED, value);
                }
            }
        }

        private bool _isDamned;
        public bool IsDamned
        {
            get => _isDamned;
            set
            {
                if (SetProperty(ref _isDamned, value))
                {
                    UpdateAttributesValue(Constants.ATTR_DAMNED, value);
                }
            }
        }

        private bool _isBlessed;
        public bool IsBlessed
        {
            get => _isBlessed;
            set
            {
                if (SetProperty(ref _isBlessed, value))
                {
                    UpdateAttributesValue(Constants.ATTR_BLESSED, value);
                }
            }
        }

        private bool _isSacred;
        public bool IsSacred
        {
            get => _isSacred;
            set
            {
                if (SetProperty(ref _isSacred, value))
                {
                    UpdateAttributesValue(Constants.ATTR_SACRED, value);
                }
            }
        }

        private bool _isForSale;
        public bool IsForSale
        {
            get => _isForSale;
            set
            {
                if (SetProperty(ref _isForSale, value))
                {
                    UpdateAttributesValue(Constants.ATTR_FORSALE, value);
                }
            }
        }

        private bool _isStolen;
        public bool IsStolen
        {
            get => _isStolen;
            set
            {
                if (SetProperty(ref _isStolen, value))
                {
                    UpdateAttributesValue(Constants.ATTR_STOLEN, value);
                }
            }
        }

        private bool _canDecay;
        public bool CanDecay
        {
            get => _canDecay;
            set
            {
                if (SetProperty(ref _canDecay, value))
                {
                    UpdateAttributesValue(Constants.ATTR_CAN_DECAY, value);
                }
            }
        }

        private bool _isStatics;
        public bool IsStatics
        {
            get => _isStatics;
            set
            {
                if (SetProperty(ref _isStatics, value))
                {
                    UpdateAttributesValue(Constants.ATTR_STATIC, value);
                }
            }
        }

        private string _currentAttributesValue;
        public string CurrentAttributesValue
        {
            get => _currentAttributesValue;
            set => SetProperty(ref _currentAttributesValue, value);
        }
        #endregion

        #region Types, Misc, Tags Properties
        private string _miscValue;
        public string MiscValue
        {
            get => _miscValue;
            set => SetProperty(ref _miscValue, value);
        }

        private string _tagsValue;
        public string TagsValue
        {
            get => _tagsValue;
            set => SetProperty(ref _tagsValue, value);
        }

        public ObservableCollection<string> Types { get; set; }
        private string _selectedType;
        public string SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        public ObservableCollection<string> MiscOptions { get; set; }
        private string _selectedMiscOption;
        public string SelectedMiscOption
        {
            get => _selectedMiscOption;
            set => SetProperty(ref _selectedMiscOption, value);
        }

        public ObservableCollection<string> Tags { get; set; }
        private string _selectedTag;
        public string SelectedTag
        {
            get => _selectedTag;
            set => SetProperty(ref _selectedTag, value);
        }
        #endregion

        #region Color Properties
        public ObservableCollection<ColorCellViewModel> QuickColorPalette { get; set; }
        public ObservableCollection<System.Windows.Media.Color> Spectrum { get; set; }

        private ColorCellViewModel? _selectedColor;
        public ColorCellViewModel? SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (SetProperty(ref _selectedColor, value))
                {
                    UpdateSpectrum();
                }
            }
        }
        #endregion

        #region Commands
        public ICommand ResetAttributesCommand { get; private set; }
        public ICommand SetAttributesCommand { get; private set; }
        public ICommand SetTypesCommand { get; private set; }
        public ICommand SetEventsCommand { get; private set; }
        public ICommand SetMiscCommand { get; private set; }
        public ICommand SetTagCommand { get; private set; }
        public ICommand OpenDoorWizardCommand { get; private set; }
        public ICommand OpenLightWizardCommand { get; private set; }
        public ICommand OpenColorSelectorCommand { get; private set; }
        public ICommand SetColorCommand { get; private set; }
        public ICommand SelectColorCommand { get; private set; }
        public ICommand ResetHueCommand { get; private set; }
        public ICommand UpdatePreviewCommand { get; private set; }
        public ICommand ApplyHueToPaletteCommand { get; private set; } // New command
        public ICommand SavePaletteCommand { get; private set; }
        public ICommand LoadPaletteCommand { get; private set; }
        #endregion

        public ItemTweakTabViewModel(UoClientCommunicator uoClientCommunicator, ScriptParser scriptParser, IDialogService dialogService, IUoArtService uoArtService, AllSettings settings, EventAggregator eventAggregator, MulFileManager mulFileManager, BodyDefService bodyDefService, LightDataService lightDataService)
        {
            _uoClientCommunicator = uoClientCommunicator;
            _scriptParser = scriptParser;
            _dialogService = dialogService;
            _uoArtService = uoArtService;
            _settings = settings;
            _eventAggregator = eventAggregator;
            _mulFileManager = mulFileManager;
            _bodyDefService = bodyDefService;
            _lightDataService = lightDataService;
            _eventAggregator.Subscribe(this);

            _currentAttributesValue = "00000";
            _miscValue = string.Empty;
            _tagsValue = string.Empty;
            _selectedType = string.Empty;
            _selectedMiscOption = string.Empty;
            _selectedTag = string.Empty;

            InitializeCollections();
            InitializeCommands();
            InitializeDefaultValues();

            LoadComboBoxData();
            // LoadQuickColorPaletteFromFile(); // Moved to Handle(ProfileLoadedEvent)
        }

        public void Handle(ProfileLoadedEvent message)
        {
            if (message.LoadedProfile != null)
            {
                _mulFileManager.Load(
                    _settings.FilePathsSettings.ArtIdx,
                    _settings.FilePathsSettings.ArtMul,
                    _settings.FilePathsSettings.HuesMul,
                    _settings.FilePathsSettings.AnimIdx,
                    _settings.FilePathsSettings.AnimMul,
                    _bodyDefService,
                    _settings.OverridePathsSettings.FilePaths
                );
                OnLoadPaletteFromFile(); // Load palette after UoArtService is ready
            }
        }

        public void Handle(SObjectSelectedEvent message)
        {
            SelectedItem = message.SelectedObject;
        }
        private string _previewHue = string.Empty;
        public string PreviewHue
        {
            get => _previewHue;
            set { SetProperty(ref _previewHue, value); UpdatePreview(); }
        }

        private string _previewObjectId = string.Empty;
        public string PreviewObjectId
        {
            get { return _previewObjectId; }
            set { SetProperty(ref _previewObjectId, value); UpdatePreview(); }
        }

        private void UpdatePreview()
        {
            if (SelectedItem == null)
            {
                ItemPreviewImage = null;
                return;
            }

            string idString = SelectedItem.DisplayId ?? SelectedItem.Id;
            uint itemId = 0;

            if (!string.IsNullOrEmpty(idString))
            {
                itemId = idString.AllToUInt();
            }

            if (itemId > 0)
            {
                int hue = 0;
                if (!string.IsNullOrEmpty(PreviewHue) && PreviewHue.StartsWith("0x"))
                {
                    int.TryParse(PreviewHue.Substring(2), NumberStyles.HexNumber, null, out hue);
                }
                else if (!string.IsNullOrEmpty(PreviewHue))
                {
                    int.TryParse(PreviewHue, out hue);
                }
                else if (!string.IsNullOrEmpty(SelectedItem.Color) && SelectedItem.Color.StartsWith("0x"))
                {
                    int.TryParse(SelectedItem.Color.Substring(2), NumberStyles.HexNumber, null, out hue);
                }


                var artRecord = _mulFileManager.GetArtRecord(itemId);

                if (artRecord != null)
                {
                    ItemPreviewImage = _mulFileManager.CreateBitmapSource(artRecord, hue);
                }
                else
                {
                    ItemPreviewImage = null;
                }
            }
            else
            {
                ItemPreviewImage = null;
            }
        }

        private void UpdatePreviewFromSelectedItem(SObject? item)
        {
            if (item == null)
            {
                PreviewObjectId = string.Empty;
                PreviewHue = string.Empty;
            }
            else
            {
                PreviewObjectId = item.Id;
                PreviewHue = item.Color;
            }
            UpdatePreview();
        }

        private void InitializeCollections()
        {
            Types = new ObservableCollection<string>();
            MiscOptions = new ObservableCollection<string>();
            Tags = new ObservableCollection<string>();
            QuickColorPalette = new ObservableCollection<ColorCellViewModel>();
            Spectrum = new ObservableCollection<System.Windows.Media.Color>();
        }

        private void InitializeCommands()
        {
            ResetAttributesCommand = new RelayCommand(OnResetAttributes);
            SetAttributesCommand = new RelayCommand(OnSetAttributes);
            SetTypesCommand = new RelayCommand(OnSetTypes);
            SetEventsCommand = new RelayCommand(OnSetEvents);
            SetMiscCommand = new RelayCommand(OnSetMisc);
            SetTagCommand = new RelayCommand(OnSetTag);
            OpenDoorWizardCommand = new RelayCommand(OnOpenDoorWizard);
            OpenLightWizardCommand = new RelayCommand(OnOpenLightWizard);
            OpenColorSelectorCommand = new RelayCommand(OnOpenColorSelector);
            SetColorCommand = new RelayCommand(OnSetColor);
            SelectColorCommand = new RelayCommand<ColorCellViewModel>(OnSelectColor);
            ResetHueCommand = new RelayCommand(OnResetHue);
            UpdatePreviewCommand = new RelayCommand(UpdatePreview);
            SavePaletteCommand = new RelayCommand(OnSavePalette); // Renamed
            LoadPaletteCommand = new RelayCommand(OnLoadPalette); // New
            ApplyHueToPaletteCommand = new RelayCommand<ColorCellViewModel>(OnApplyHueToPalette);
        }

        private void InitializeDefaultValues()
        {
            CurrentAttributesValue = "00000";
            MiscValue = "";
            TagsValue = "";
        }

        private void LoadComboBoxData()
        {
            foreach (var type in _scriptParser.ItemTypes) Types.Add(type);
            SelectedType = Types.FirstOrDefault() ?? string.Empty;

            foreach (var misc in _scriptParser.ItemProps) MiscOptions.Add(misc);
            SelectedMiscOption = MiscOptions.FirstOrDefault() ?? string.Empty;

            foreach (var tag in _scriptParser.ItemTags) Tags.Add(tag);
            SelectedTag = Tags.FirstOrDefault() ?? string.Empty;
        }

        private void LoadQuickColorPalette()
        {
            // In a real scenario, these would be loaded from settings/registry like in the C++ version
            ushort[] defaultPalette = new ushort[] {
                1, 7, 13, 19, 25, 31, 37, 43,
                49, 55, 61, 67, 73, 79, 85, 91,
                97, 103, 109, 115, 121, 127, 133, 139,
                145, 151, 157, 163, 169, 175, 181, 187,
                193, 199, 205, 211, 217, 223, 229, 235,
                241, 247, 253, 259, 265, 271, 277, 283,
                289, 295, 301, 307, 313, 319, 325, 331,
                337, 343, 349, 355, 361, 367, 373, 379
            };

            for (int i = 0; i < 64; i++)
            {
                ushort colorIndex = defaultPalette[i];
                var cell = new ColorCellViewModel
                {
                    ColorIndex = colorIndex,
                    Color = _uoArtService.GetColorFromHue(colorIndex, 16) // Get a mid-range shade for the palette view
                };
                QuickColorPalette.Add(cell);
            }

            SelectedColor = QuickColorPalette.FirstOrDefault();
        }

        private async void OnSavePalette() // Renamed from SaveQuickColorPalette
        {
            try
            {
                string paletteName = await _dialogService.ShowInputDialog("Save Palette", "Enter a name for the palette:", "MyPalette");

                if (string.IsNullOrEmpty(paletteName))
                {
                    MessageBox.Show("Palette name cannot be empty.", "Save Palette", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string palettesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Palettes");
                Directory.CreateDirectory(palettesDirectory); // Ensure the directory exists

                string filePath = Path.Combine(palettesDirectory, $"{paletteName}.json");

                var colorsToSave = QuickColorPalette.Select(c => c.ColorIndex).ToList();
                string jsonString = JsonSerializer.Serialize(colorsToSave, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonString);

                _isPaletteModified = false;
                Logger.Log($"Quick color palette saved to {filePath}");
                MessageBox.Show($"Palette saved as {paletteName}", "Palette Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error saving quick color palette: {ex.Message}");
                MessageBox.Show($"Error saving palette: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnLoadPalette() // New method
        {
            try
            {
                string palettesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Palettes");
                if (!Directory.Exists(palettesDirectory))
                {
                    MessageBox.Show("No saved palettes found.", "Load Palette", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var paletteFiles = Directory.GetFiles(palettesDirectory, "*.json")
                                            .Select(Path.GetFileNameWithoutExtension)
                                            .ToList();

                if (!paletteFiles.Any())
                {
                    MessageBox.Show("No saved palettes found.", "Load Palette", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ObservableCollection<string> observablePaletteFiles = new ObservableCollection<string>(paletteFiles);

                string? selectedPaletteName = await _dialogService.ShowSelectListDialog("Load Palette", "Select a palette to load:", observablePaletteFiles);

                if (!string.IsNullOrEmpty(selectedPaletteName))
                {
                    string filePath = Path.Combine(palettesDirectory, $"{selectedPaletteName}.json");
                    string jsonString = File.ReadAllText(filePath);
                    var loadedColors = JsonSerializer.Deserialize<List<ushort>>(jsonString);

                    if (loadedColors != null && loadedColors.Any())
                    {
                        QuickColorPalette.Clear();
                        foreach (var colorIndex in loadedColors)
                        {
                            var cell = new ColorCellViewModel
                            {
                                ColorIndex = colorIndex,
                                Color = _uoArtService.GetColorFromHue(colorIndex, 16)
                            };
                            QuickColorPalette.Add(cell);
                        }
                        Logger.Log($"Quick color palette loaded from {filePath}");
                        MessageBox.Show($"Palette '{selectedPaletteName}' loaded successfully.", "Palette Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Selected palette file is empty or corrupted.", "Load Palette Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading quick color palette: {ex.Message}");
                MessageBox.Show($"Error loading palette: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoadPaletteFromFile() // Renamed from LoadQuickColorPaletteFromFile
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quick_palette.json");
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var loadedColors = JsonSerializer.Deserialize<List<ushort>>(jsonString);
                    if (loadedColors != null && loadedColors.Any())
                    {
                        QuickColorPalette.Clear();
                        foreach (var colorIndex in loadedColors)
                        {
                            var cell = new ColorCellViewModel
                            {
                                ColorIndex = colorIndex,
                                Color = _uoArtService.GetColorFromHue(colorIndex, 16)
                            };
                            QuickColorPalette.Add(cell);
                        }
                        Logger.Log("Quick color palette loaded from file.");
                    }
                }
                else
                {
                    // If file doesn't exist, load default palette
                    LoadQuickColorPalette();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading quick color palette from file: {ex.Message}. Loading default palette.");
                LoadQuickColorPalette(); // Fallback to default palette on error
            }
        }

        private void UpdateSpectrum()
        {
            Spectrum.Clear();
            if (SelectedColor == null) return;

            for (int i = 0; i < 32; i++)
            {
                Spectrum.Add(_uoArtService.GetColorFromHue(SelectedColor.ColorIndex, i));
            }
        }

        private void UpdateAttributesValue(int attributeFlag, bool isSet)
        {
            try
            {
                uint currentVal = Convert.ToUInt32(CurrentAttributesValue, 16);
                if (isSet)
                {
                    currentVal |= (uint)attributeFlag;
                }
                else
                {
                    currentVal &= ~(uint)attributeFlag;
                }
                CurrentAttributesValue = currentVal.ToString("X5");
            }
            catch (FormatException ex)
            {
                Logger.Log($"[ItemTweakTabViewModel] Invalid hex value for CurrentAttributesValue: {ex.Message}");
            }
        }

        #region Command Handlers
        private void OnResetHue()
        {
            PreviewHue = string.Empty;
        }

        private void OnSelectColor(ColorCellViewModel colorCell)
        {
            SelectedColor = colorCell;
            if (colorCell != null)
            {
                PreviewHue = $"0x{colorCell.ColorIndex:X}";
            }
        }

        private async void OnSetColor()
        {
            if (SelectedColor != null)
            {
                await _uoClientCommunicator.SendToUOAsync($"set color {SelectedColor.ColorIndex:X4}");
                Logger.Log($"[ItemTweakTabViewModel] Set Color: {SelectedColor.ColorIndex:X4}");
            }
        }

        private async void OnOpenColorSelector()
        {
            var colorSelectionViewModel = new ColorSelectionViewModel(_uoArtService, SelectedItem);
            bool? result = await _dialogService.ShowDialog(colorSelectionViewModel);

            if (result == true && colorSelectionViewModel.SelectedHue != null)
            {
                PreviewHue = $"0x{colorSelectionViewModel.SelectedColorIndex:X}";

                // Find if the color is in the palette, if so, select it.
                var existingCell = QuickColorPalette.FirstOrDefault(c => c.ColorIndex == colorSelectionViewModel.SelectedHue.ColorIndex);
                if (existingCell != null)
                {
                    SelectedColor = existingCell;
                }
                else
                {
                    // If not in palette, create a temporary cell for selection
                    SelectedColor = new ColorCellViewModel
                    {
                        ColorIndex = colorSelectionViewModel.SelectedHue.ColorIndex,
                        Color = _uoArtService.GetColorFromHue(colorSelectionViewModel.SelectedHue.ColorIndex, 16)
                    };
                }
            }
        }

        private void OnResetAttributes()
        {
            IsIdentified = false;
            IsDecay = false;
            IsNewbie = false;
            IsAlwaysMoveable = false;
            IsNeverMoveable = false;
            IsMagic = false;
            IsOwnedByTown = false;
            IsInvisible = false;
            IsCursed = false;
            IsDamned = false;
            IsBlessed = false;
            IsSacred = false;
            IsForSale = false;
            IsStolen = false;
            CanDecay = false;
            IsStatics = false;
            CurrentAttributesValue = "00000";
        }

        private async void OnSetAttributes()
        {
            await _uoClientCommunicator.SendToUOAsync($"set attr {CurrentAttributesValue}");
        }

        private async void OnSetTypes()
        {
            if (!string.IsNullOrEmpty(SelectedType))
            {
                await _uoClientCommunicator.SendToUOAsync($"set Type {SelectedType}");
            }
        }

        private async void OnSetEvents()
        {
            if (!string.IsNullOrEmpty(SelectedMiscOption))
            {
                await _uoClientCommunicator.SendToUOAsync($"Events +{SelectedMiscOption}");
            }
        }

        private async void OnSetMisc()
        {
            if (!string.IsNullOrEmpty(SelectedMiscOption) && !string.IsNullOrEmpty(MiscValue))
            {
                string cleanedMiscOption = SelectedMiscOption.Split('(')[0].Trim();
                await _uoClientCommunicator.SendToUOAsync($"set {cleanedMiscOption} {MiscValue}");
            }
        }

        private async void OnSetTag()
        {
            if (!string.IsNullOrEmpty(SelectedTag) && !string.IsNullOrEmpty(TagsValue))
            {
                string cleanedTag = SelectedTag.Split('(')[0].Trim();
                await _uoClientCommunicator.SendToUOAsync($"set {cleanedTag} {TagsValue}");
            }
        }

        private async void OnOpenDoorWizard()
        {
            // Placeholder for Door Wizard logic
            await _dialogService.ShowDialog(new ColorSelectionViewModel(_uoArtService, SelectedItem));
        }

        private async void OnOpenLightWizard()
        {
            var lightWizardViewModel = new LightWizardViewModel(_lightDataService, _uoArtService, SelectedItem);
            bool? result = await _dialogService.ShowDialog(lightWizardViewModel);

            // Handle result if needed
        }

        public bool CheckAndSavePalette()
        {
            if (_isPaletteModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The quick color palette has been modified. Do you want to save changes before exiting?",
                    "Save Palette Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    OnSavePalette();
                    return true;
                }
                else if (result == MessageBoxResult.No)
                {
                    return true; // User chose not to save, proceed with exit
                }
                else // Cancel
                {
                    return false; // User cancelled exit
                }
            }
            return true; // Not modified, proceed with exit
        }
        #endregion

        private void OnApplyHueToPalette(ColorCellViewModel colorCell)
        {
            if (colorCell == null || SelectedColor == null) return;

            MessageBoxResult result = MessageBox.Show(
                $"Do you want to replace the color of this square ({colorCell.ColorIndex}) with the selected color (0x{SelectedColor.ColorIndex:X})?",
                "Confirm Color Replacement",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Update the ColorCellViewModel
                colorCell.ColorIndex = SelectedColor.ColorIndex;
                colorCell.Color = SelectedColor.Color; // Use the actual color from SelectedColor

                _isPaletteModified = true;
                Logger.Log($"Palette color updated: Index {colorCell.ColorIndex}, Color {colorCell.Color}");
            }
        }
        
    }
}