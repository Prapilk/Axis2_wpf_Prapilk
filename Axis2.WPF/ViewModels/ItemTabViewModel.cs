using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Threading.Tasks;
using System.Text.Json;
using Axis2.WPF.Extensions;
using MessageBox = System.Windows.MessageBox;
using Axis2.WPF.Views.Dialogs; // Added for SaveCustomListDialog
using System.Windows.Threading; // For DispatcherTimer
using System.Diagnostics; // For Stopwatch
using System.Windows.Media; // For DrawingVisual, RenderTargetBitmap, TransformGroup, ScaleTransform, RotateTransform
using System.Windows.Media.Imaging;

namespace Axis2.WPF.ViewModels
{
    public class ItemTabViewModel : BindableBase, IHandler<ProfileLoadedEvent>
    {
        

        private readonly MulFileManager _mulFileManager;
        private readonly ScriptParser _scriptParser;
        private readonly IUoClient _uoClient;
        private readonly EventAggregator _eventAggregator;
        private readonly LightDataService _lightDataService;
        private readonly UoArtService _uoArtService;
        private readonly ISettingsService _settingsService;
        private string _scriptsPath;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<SObject> DisplayedItems { get; } = new();
        public ObservableCollection<CustomItemList> CustomItemLists { get; } = new();

        private object _selectedTreeItem;
        public object SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                Logger.Log($"SelectedTreeItem setter: New value is {value?.GetType().Name ?? "null"}. _isProgrammaticTreeSelection is {_isProgrammaticTreeSelection}.");

                // If the user clicks the tree and we're in search mode,
                // we MUST update, even if the item is the same.
                if (_isShowingSearchResults && !_isProgrammaticTreeSelection)
                {
                    _isShowingSearchResults = false;
                    // We need to manually set the property and call UpdateDisplayedItems,
                    // because SetProperty will return false if the value is the same.
                    _selectedTreeItem = value;
                    OnPropertyChanged(); // for SelectedTreeItem
                    Logger.Log($"SelectedTreeItem setter: Forcing update to clear search results.");
                    UpdateDisplayedItems();
                }
                else if (SetProperty(ref _selectedTreeItem, value))
                {
                    if (!_isProgrammaticTreeSelection) // Only reset if not programmatic
                    {
                        _isShowingSearchResults = false; // Reset flag when tree selection changes
                        Logger.Log($"SelectedTreeItem setter: _isShowingSearchResults set to {_isShowingSearchResults} (non-programmatic).");
                    }
                    UpdateDisplayedItems(); // Always call UpdateDisplayedItems
                }
            }
        }

        private SObject _selectedItem;
        public SObject SelectedItem
        {
            get => _selectedItem;
            set
            {
                Logger.Log($"SelectedItem setter: New value is {value?.Description ?? "null"}.");
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    UpdateItemImage(value);
                    OnPropertyChanged(nameof(ItemImageActualWidth));
                    OnPropertyChanged(nameof(ItemImageActualHeight));
                    _eventAggregator.Publish(new SObjectSelectedEvent(value));

                    _preserveImageOnUpdate = true; // Set the flag
                    ExpandAndSelectInTreeView(value); // Call new method
                    _preserveImageOnUpdate = false; // Reset the flag
                }
            }
        }

        private CustomItemList _selectedCustomItemList;
        public CustomItemList SelectedCustomItemList
        {
            get => _selectedCustomItemList;
            set => SetProperty(ref _selectedCustomItemList, value);
        }

        private SObject _selectedItemInCustomList;
        public SObject SelectedItemInCustomList
        {
            get => _selectedItemInCustomList;
            set
            {
                if (SetProperty(ref _selectedItemInCustomList, value))
                {
                    SelectedItem = value;
                }
            }
        }

        private BitmapSource _itemImage;
        public BitmapSource ItemImage
        {
            get => _itemImage;
            set => SetProperty(ref _itemImage, value);
        }

        public double ItemImageActualWidth => ItemImage?.PixelWidth ?? 0;
        public double ItemImageActualHeight => ItemImage?.PixelHeight ?? 0;

        public bool IsItemsVisible => _settingsItemTabViewModel?.ShowItems ?? true;
        public System.Windows.Media.Brush CurrentItemBackgroundColor
        {
            get
            {
                var color = _settingsItemTabViewModel?.ItemBGColor ?? Colors.Transparent;
                return new SolidColorBrush(color);
            }
        }
        public bool IsRoomViewEnabled => _settingsItemTabViewModel?.RoomView ?? false;

        private string _zCoordinate = "0";
        public string ZCoordinate
        {
            get => _zCoordinate;
            set => SetProperty(ref _zCoordinate, value);
        }

        private string _nukeArgument = "";
        public string NukeArgument
        {
            get => _nukeArgument;
            set => SetProperty(ref _nukeArgument, value);
        }

        private string _amount = "1";
        public string Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _nudgeAmount = "1";
        public string NudgeAmount
        {
            get => _nudgeAmount;
            set => SetProperty(ref _nudgeAmount, value);
        }

        private bool _lockItem = false;
        public bool LockItem
        {
            get => _lockItem;
            set => SetProperty(ref _lockItem, value);
        }

        private string _minTime = "10";
        public string MinTime
        {
            get => _minTime;
            set => SetProperty(ref _minTime, value);
        }

        private string _maxTime = "50";
        public string MaxTime
        {
            get => _maxTime;
            set => SetProperty(ref _maxTime, value);
        }

        private string _spawnRate = "1";
        public string SpawnRate
        {
            get => _spawnRate;
            set => SetProperty(ref _spawnRate, value);
        }

        private string _maxDist = "1";
        public string MaxDist
        {
            get => _maxDist;
            set => SetProperty(ref _maxDist, value);
        }

        private string _itemIDText = "";
        public string ItemIDText
        {
            get => _itemIDText;
            set => SetProperty(ref _itemIDText, value);
        }

        private string _itemIDDecText = "";
        public string ItemIDDecText
        {
            get => _itemIDDecText;
            set => SetProperty(ref _itemIDDecText, value);
        }

        public ICommand CreateCommand { get; private set; }
        public ICommand TileCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand FlipCommand { get; private set; }
        public ICommand NukeCommand { get; private set; }
        public ICommand NudgeUpCommand { get; private set; }
        public ICommand NudgeDownCommand { get; private set; }
        public ICommand MoveCommand { get; private set; }
        public ICommand InitSpawnCommand { get; private set; }
        public ICommand AddItemCommand { get; private set; }
        public ICommand EditItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }
        public ICommand LoadSettingsCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand ResetSettingsCommand { get; private set; }
        public ICommand SaveCustomListCommand { get; private set; }
        public ICommand LoadCustomListCommand { get; private set; }
        public ICommand NewCustomListCommand { get; private set; }
        public ICommand UpdateTooltipCommand { get; private set; }
        public ICommand DeleteFromCustomListCommand { get; private set; }

        private Settings.SettingsItemTabViewModel _settingsItemTabViewModel;

        private readonly DispatcherTimer _lightAnimationTimer;
        private readonly Stopwatch _lightAnimationStopwatch;
        private DrawConfigEntry _currentLightDrawConfig;
        private double _currentLightZoom = 1.0;
        private double _currentLightRotation = 0.0;
        private int _currentLightColorIndex = 0;

        private bool _isLightSourceEnabled;
        public bool IsLightSourceEnabled
        {
            get => _isLightSourceEnabled;
            set
            {
                if (SetProperty(ref _isLightSourceEnabled, value))
                {
                    if (value)
                    {
                        _lightAnimationStopwatch.Restart();
                        _lightAnimationTimer.Start();
                    }
                    else
                    {
                        _lightAnimationTimer.Stop();
                        _lightAnimationStopwatch.Stop();
                    }
                    UpdateLightSourceImage(); // Update light image separately
                }
            }
        }

        public double ItemImageXOffset => 0; // Placeholder for now
        public double ItemImageYOffset => 0; // Placeholder for now

        public ItemTabViewModel(MulFileManager mulFileManager, ScriptParser scriptParser, EventAggregator eventAggregator, IUoClient uoClient, Settings.SettingsItemTabViewModel settingsItemTabViewModel, LightDataService lightDataService, UoArtService uoArtService, ISettingsService settingsService)
        {
            _mulFileManager = mulFileManager;
            _scriptParser = scriptParser;
            _uoClient = uoClient;
            _eventAggregator = eventAggregator;
            _lightDataService = lightDataService;
            _uoArtService = uoArtService;
            _settingsService = settingsService;
            _eventAggregator.Subscribe(this);

            // Initialize timer and stopwatch
            _lightAnimationTimer = new DispatcherTimer();
            _lightAnimationTimer.Interval = TimeSpan.FromMilliseconds(50); // 20 FPS
            _lightAnimationTimer.Tick += LightAnimationTimer_Tick;
            _lightAnimationStopwatch = new Stopwatch();

            CreateCommand = new RelayCommand(OnCreate);
            TileCommand = new RelayCommand(OnTile);
            RemoveCommand = new RelayCommand(OnRemove);
            FlipCommand = new RelayCommand(OnFlip);
            NukeCommand = new RelayCommand(OnNuke);
            NudgeUpCommand = new RelayCommand(OnNudgeUp);
            NudgeDownCommand = new RelayCommand(OnNudgeDown);
            MoveCommand = new RelayCommand<string>(OnMove);
            InitSpawnCommand = new RelayCommand(OnInitSpawn);
            SaveCustomListCommand = new RelayCommand(OnSaveCustomList);
            LoadCustomListCommand = new RelayCommand(OnLoadCustomList);
            NewCustomListCommand = new RelayCommand(OnNewCustomList);
            UpdateTooltipCommand = new RelayCommand(OnUpdateTooltip);
            DeleteFromCustomListCommand = new RelayCommand(OnDeleteFromCustomList);

            _settingsItemTabViewModel = settingsItemTabViewModel;

            // Initialize _settingsItemTabViewModel from SettingsService
            var loadedSettings = _settingsService.LoadSettings();
            _settingsItemTabViewModel = loadedSettings?.ItemTabSettings ?? new Settings.SettingsItemTabViewModel();

            // Subscribe to SettingsChanged event from SettingsService
            _settingsService.SettingsChanged += (s, e) =>
            {
                _settingsItemTabViewModel = e.AllSettings.ItemTabSettings;
                OnPropertyChanged(nameof(IsItemsVisible));
                OnPropertyChanged(nameof(CurrentItemBackgroundColor));
                OnPropertyChanged(nameof(IsRoomViewEnabled));
            };

            // Also subscribe to PropertyChanged on _settingsItemTabViewModel itself for direct changes
            _settingsItemTabViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.SettingsItemTabViewModel.ShowItems))
                {
                    OnPropertyChanged(nameof(IsItemsVisible));
                }
                else if (e.PropertyName == nameof(Settings.SettingsItemTabViewModel.ItemBGColor))
                {
                    OnPropertyChanged(nameof(CurrentItemBackgroundColor));
                }
                else if (e.PropertyName == nameof(Settings.SettingsItemTabViewModel.RoomView))
                {
                    OnPropertyChanged(nameof(IsRoomViewEnabled));
                }
            };
        }

        

        private void OnCreate()
        {
            if (SelectedItem == null)
            {
                //System.Windows.MessageBox.Show("No item selected.", "Error");
                return;
            }
            string command = LockItem ? $"static {SelectedItem.Id}" : $"add {SelectedItem.Id}";
            _uoClient.SendToClient(command);
        }

        private void OnTile()
        {
            if (SelectedItem == null)
            {
                //System.Windows.MessageBox.Show("No item selected.", "Error");
                return;
            }
            string command = $"tile {ZCoordinate} {SelectedItem.Id}";
            _uoClient.SendToClient(command);
        }

        private void OnRemove()
        {
            _uoClient.SendToClient("remove");
        }

        private void OnFlip()
        {
            _uoClient.SendToClient("xflip");
        }

        private void OnNuke()
        {
            string command = $"nuke {NukeArgument}";
            _uoClient.SendToClient(command);
        }

        private void OnNudgeUp()
        {
            string command = $"nudgeup {NudgeAmount}";
            _uoClient.SendToClient(command);
        }

        private void OnNudgeDown()
        {
            string command = $"nudgedown {NudgeAmount}";
            _uoClient.SendToClient(command);
        }

        private void OnMove(string moveDirection)
        {
            string command = "";
            string moveValue = NudgeAmount;
            if (string.IsNullOrEmpty(moveValue))
                moveValue = "0";

            switch (moveDirection)
            {
                case "Move1": // Up
                    command = $"xmove -{moveValue} -{moveValue}";
                    
                    break;
                case "Move2": // Up-Right
                    command = $"xmove 0 -{moveValue}";
                    
                    break;
                case "Move3": // Right
                    command = $"xmove {moveValue} -{moveValue}";
                    
                    break;
                case "Move4": // Down-Right
                    command = $"xmove {moveValue} 0";
                    
                    break;
                case "Move5": // Down
                    command = $"xmove {moveValue} {moveValue}";
                    //command = $"xmove 0 {moveValue}";
                    break;
                case "Move6": // Down-Left
                    command = $"xmove 0 {moveValue}";
                    
                    break;
                case "Move7": // Left
                    command = $"xmove -{moveValue} {moveValue}";
                   
                    break;
                case "Move8": // Up-Left
                    command = $"xmove -{moveValue} 0";
                    
                    break;
            }
            _uoClient.SendToClient(command);
        }

        private async void OnInitSpawn()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("No item is selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!int.TryParse(Amount, out int iAmount))
            {
                iAmount = 1;
            }
            if (iAmount == 0)
            {
                iAmount = 1;
            }

            if (!int.TryParse(MaxDist, out int iMaxDist))
            {
                iMaxDist = 1;
            }

            if (!int.TryParse(MinTime, out int iMinTime))
            {
                iMinTime = 10; // Default from C++ example
            }

            if (!int.TryParse(MaxTime, out int iMaxTime))
            {
                iMaxTime = 50; // Default from C++ example
            }

            if (iMaxTime <= iMinTime)
            {
                iMaxTime = iMinTime + 1;
            }

            // This logic combines the "Place" approach (using a temporary worldgem)
            // with the correct command sequence for item spawners.
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);
            _uoClient.SendToClient("add 01ea7"); // Add the "worldgem" to mark the location.

            // Add delays to ensure the server processes the "add" command before we continue.
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            // Countdown for manual placement confirmation
            for (double i = 3.0; i >= 0.0; i -= 0.5)
            {
                _uoClient.SendToClient($"hear {i:F1}"); // Send countdown message
                await Task.Delay(250);
            }

            _uoClient.SendToClient($"act.type {Constants.ITEM_SPAWN_ITEM}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.amount {iAmount}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            // Use the string ID for items, not the numeric conversion.
            _uoClient.SendToClient($"act.more {SelectedItem.Id}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            // Re-add the essential "more2" command for the spawn rate.
            _uoClient.SendToClient($"act.more2 {SpawnRate}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.morep {iMinTime} {iMaxTime} {iMaxDist}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

           

            _uoClient.SendToClient($"act.attr {Constants.ATTR_INVIS | Constants.ATTR_MAGIC | Constants.ATTR_MOVE_NEVER:X4}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.timer 1");
        }

        private void OnSaveCustomList()
        {
            var dialog = new SaveCustomListDialog();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                string listName = dialog.ListName;
                if (string.IsNullOrWhiteSpace(listName))
                {
                    MessageBox.Show("List name cannot be empty.", "Save Custom List", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // If no custom list is selected, create a new one
                if (SelectedCustomItemList == null)
                {
                    SelectedCustomItemList = new CustomItemList { Name = listName };
                    CustomItemLists.Add(SelectedCustomItemList);
                }
                else
                {
                    // If a list is selected, update its name if it was changed
                    SelectedCustomItemList.Name = listName;
                }

                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{listName}.json");
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(SelectedCustomItemList, options);
                    File.WriteAllText(filePath, jsonString);
                    MessageBox.Show($"Custom list '{listName}' saved successfully!", "Save Custom List");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving custom list: {ex.Message}", "Save Custom List Error");
                }
            }
        }

        private void OnNewCustomList()
        {
            // Create a new empty custom list and set it as selected
            var newCustomList = new CustomItemList { Name = "New Custom List" }; // Default name
            CustomItemLists.Add(newCustomList);
            SelectedCustomItemList = newCustomList;
            MessageBox.Show("New custom list created. You can now drag items into it.", "New Custom List", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void OnUpdateTooltip()
        {
            _uoClient.SendToClient("update");
        }

        private void OnDeleteFromCustomList()
        {
            if (SelectedCustomItemList != null && SelectedItemInCustomList != null)
            {
                SelectedCustomItemList.Items.Remove(SelectedItemInCustomList);
            }
        }

        private void OnLoadCustomList()
        {
            var dialog = new LoadCustomListDialog();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                string listName = dialog.SelectedListName;
                if (string.IsNullOrWhiteSpace(listName))
                {
                    MessageBox.Show("No list selected.", "Load Custom List", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{listName}.json");
                    if (File.Exists(filePath))
                    {
                        string jsonString = File.ReadAllText(filePath);
                        var loadedList = JsonSerializer.Deserialize<CustomItemList>(jsonString);
                        if (loadedList != null)
                        {
                            // Check if the list already exists in CustomItemLists collection
                            var existingList = CustomItemLists.FirstOrDefault(l => l.Name == loadedList.Name);
                            if (existingList != null)
                            {
                                // Update existing list
                                existingList.Items.Clear();
                                foreach (var item in loadedList.Items)
                                {
                                    existingList.Items.Add(item);
                                }
                                SelectedCustomItemList = existingList;
                            }
                            else
                            {
                                // Add new list
                                CustomItemLists.Add(loadedList);
                                SelectedCustomItemList = loadedList;
                            }
                            MessageBox.Show($"Custom list '{listName}' loaded successfully!", "Load Custom List");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"File '{listName}.json' not found.", "Load Custom List", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading custom list: {ex.Message}", "Load Custom List Error");
                }
            }
        }

        public void Handle(ProfileLoadedEvent message)
        {
            Logger.Log("--- ItemTabViewModel: OnProfileLoaded event received ---");
            if (message?.LoadedProfile == null)
            {
                Logger.Log("ERROR: ItemTabViewModel received a null profile.");
                return;
            }
            _scriptsPath = message.LoadedProfile.BaseDirectory;
            LoadScripts();
        }

        private void LoadScripts()
        {
            Logger.Log("DEBUG: ItemTabViewModel.LoadScripts method entered.");
            Categories.Clear();
            DisplayedItems.Clear();
            ItemImage = null;

            if (string.IsNullOrEmpty(_scriptsPath) || !Directory.Exists(_scriptsPath))
            {
                Logger.Log($"ERROR: ItemTabViewModel - ScriptsPath is null or does not exist: '{_scriptsPath}'.");
                return;
            }

            var allItems = new List<SObject>();
            var scriptFiles = Directory.GetFiles(_scriptsPath, "*.scp", SearchOption.AllDirectories);
            Logger.Log($"DEBUG: ItemTabViewModel - Found {scriptFiles.Length} script files in '{_scriptsPath}'.");

            foreach (var file in scriptFiles)
            {
                Logger.Log($"DEBUG: ItemTabViewModel - Parsing file: {file}");
                allItems.AddRange(_scriptParser.ParseFile(file));
            }

            var itemDefs = allItems.Where(item => item.Type == SObjectType.Item).ToList();
            var categorizedItems = _scriptParser.Categorize(itemDefs);

            foreach (var category in categorizedItems)
            {
                Categories.Add(category);
            }
            Logger.Log($"DEBUG: ItemTabViewModel - Finished loading and categorizing {itemDefs.Count} items.");
        }

        private void UpdateDisplayedItems()
        {
            Logger.Log("UpdateDisplayedItems: Method entered.");
            Logger.Log($"UpdateDisplayedItems: _isShowingSearchResults is {_isShowingSearchResults}.");

            if (_isShowingSearchResults)
            {
                // If currently showing search results, do not clear or repopulate based on tree selection
                // The search results should persist until a new search or tree selection is made
                Logger.Log("UpdateDisplayedItems: _isShowingSearchResults is true, returning.");
                return;
            }

            DisplayedItems.Clear();
            Logger.Log("UpdateDisplayedItems: DisplayedItems cleared.");
            if (!_preserveImageOnUpdate) // Check the flag
            {
                ItemImage = null;
            }

            if (SelectedTreeItem is SubCategory subCategory)
            {
                foreach (var item in subCategory.Items)
                {
                    DisplayedItems.Add(item);
                }
                Logger.Log($"UpdateDisplayedItems: Added {DisplayedItems.Count} items from selected subcategory.");
            }
        }

        private void UpdateItemImage(SObject item)
        {
            if (item == null)
            {
                ItemImage = null;
                ItemIDText = "";
                ItemIDDecText = "";
                return;
            }

            string idString = item.DisplayId ?? item.Id;
            uint itemId = idString.AllToUInt();

            if (itemId > 0)
            {
                int hue = 0;
                if (!string.IsNullOrEmpty(item.Color) && item.Color.StartsWith("0x"))
                {
                    int.TryParse(item.Color.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out hue);
                }

                ItemIDText = $"0x{itemId:X}";
                ItemIDDecText = $"ID:{itemId}    Hue:0x{hue:X}";

                var artRecord = _mulFileManager.GetArtRecord(itemId);
                BitmapSource baseItemImage = null;

                if (artRecord != null)
                {
                    baseItemImage = _mulFileManager.CreateBitmapSource(artRecord, hue);
                }

                ItemImage = baseItemImage; // Always set ItemImage to the base item image

                // Handle light source separately and initialize light config
                _lightDataService.TileDataItems.TryGetValue(itemId, out var tileDataItem);
                if (IsLightSourceEnabled && tileDataItem != null && (tileDataItem.Flags & 0x00800000) != 0)
                {
                    var lightColorEntry = _lightDataService.LightColors.Values.FirstOrDefault(lc => lc.Id == itemId);
                    if (lightColorEntry != null)
                    {
                        _currentLightDrawConfig = _lightDataService.DrawConfigs.FirstOrDefault(dc => dc.Id == lightColorEntry.DrawConfigId);
                        _currentLightColorIndex = 0; // Reset color index for new item
                    }
                    else
                    {
                        _currentLightDrawConfig = null;
                    }
                }
                else
                {
                    _currentLightDrawConfig = null;
                }

                UpdateLightSourceImage(); // Call this to update the light source image and its offsets
            }
            else
            {
                ItemImage = null;
                LightSourceImage = null;
                ItemIDText = "";
                ItemIDDecText = "";
                _currentLightDrawConfig = null;
            }
        }

        private BitmapSource _lightSourceImage;
        public BitmapSource LightSourceImage
        {
            get => _lightSourceImage;
            set => SetProperty(ref _lightSourceImage, value);
        }

        private double _itemImageLightXOffset;
        public double ItemImageLightXOffset
        {
            get => _itemImageLightXOffset;
            set => SetProperty(ref _itemImageLightXOffset, value);
        }

        private double _itemImageLightYOffset;
        public double ItemImageLightYOffset
        {
            get => _itemImageLightYOffset;
            set => SetProperty(ref _itemImageLightYOffset, value);
        }

        private void LightAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_currentLightDrawConfig == null) return;

            float elapsedTime = (float)_lightAnimationStopwatch.Elapsed.TotalSeconds;

            // Color alternation
            if (_currentLightDrawConfig.Alternance > 0 && _currentLightDrawConfig.ColorIds.Count > 1)
            {
                int newIndex = (int)(elapsedTime / _currentLightDrawConfig.Alternance) % _currentLightDrawConfig.ColorIds.Count;
                if (newIndex != _currentLightColorIndex)
                {
                    _currentLightColorIndex = newIndex;
                    UpdateLightSourceImage(); // Update only the light source image
                }
            }

            // Zoom
            if (_currentLightDrawConfig.TimeZoom > 0)
            {
                _currentLightZoom = _currentLightDrawConfig.Dezoom + ((_currentLightDrawConfig.Zoom - _currentLightDrawConfig.Dezoom) / 2) * (0.5f * (1 + Math.Sin(elapsedTime * _currentLightDrawConfig.TimeZoom)));
            }
            else
            {
                _currentLightZoom = _currentLightDrawConfig.Zoom;
            }
            if (_currentLightZoom < _currentLightDrawConfig.Dezoom)
            {
                _currentLightZoom = _currentLightDrawConfig.Dezoom;
            }

            // Rotation
            if (_currentLightDrawConfig.Rotation > 0)
            {
                _currentLightRotation = (elapsedTime / _currentLightDrawConfig.Rotation) * 360.0f;
            }

            // Re-render the light source image with updated transformations
            UpdateLightSourceImage();
        }

        private void UpdateLightSourceImage()
        {
            if (SelectedItem == null || !IsLightSourceEnabled)
            {
                LightSourceImage = null;
                LightSourceTranslateY = 0; // Reset offset
                return;
            }

            string idString = SelectedItem.DisplayId ?? SelectedItem.Id;
            uint itemId = idString.AllToUInt();

            _lightDataService.TileDataItems.TryGetValue(itemId, out var tileDataItem);
            if (tileDataItem == null || (tileDataItem.Flags & 0x00800000) == 0)
            {
                LightSourceImage = null;
                LightSourceTranslateY = 0; // Reset offset
                return;
            }

            var lightColorEntry = _lightDataService.LightColors.Values.FirstOrDefault(lc => lc.Id == itemId);
            if (lightColorEntry == null)
            {
                LightSourceImage = null;
                LightSourceTranslateY = 0; // Reset offset
                return;
            }

            ushort lightId = tileDataItem.Quality;
            ushort colorId = 0;

            if (_currentLightDrawConfig != null && _currentLightDrawConfig.ColorIds.Any())
            {
                colorId = _currentLightDrawConfig.ColorIds[_currentLightColorIndex];
            }

            if (lightId > 0)
            {
                BitmapSource lightImage = _uoArtService.GetLightImage(lightId, colorId);
                if (lightImage != null)
                {
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        TransformGroup transformGroup = new TransformGroup();
                        transformGroup.Children.Add(new ScaleTransform(_currentLightZoom, _currentLightZoom, lightImage.PixelWidth / 2, lightImage.PixelHeight / 2));
                        transformGroup.Children.Add(new RotateTransform(_currentLightRotation, lightImage.PixelWidth / 2, lightImage.PixelHeight / 2));
                        drawingContext.PushTransform(transformGroup);
                        drawingContext.DrawImage(lightImage, new Rect(0, 0, lightImage.PixelWidth, lightImage.PixelHeight));
                        drawingContext.Pop();
                    }
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                        lightImage.PixelWidth, lightImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render(drawingVisual);

                    // Recalculate offsets based on the current base item image size
                    if (ItemImage != null)
                    {
                        ItemImageLightXOffset = (ItemImage.PixelWidth / 2) - (renderTargetBitmap.PixelWidth / 2);
                        LightSourceImage = renderTargetBitmap;
                        LightSourceTranslateY = 0; // Center vertically on the item
                    }
                    else
                    {
                        LightSourceImage = null;
                        LightSourceTranslateY = 0; // Reset offset
                    }
                }
                else
                {
                    LightSourceImage = null;
                    LightSourceTranslateY = 0; // Reset offset
                }
            }
            else
            {
                LightSourceImage = null;
                LightSourceTranslateY = 0; // Reset offset
            }
        }

        private double _lightSourceTranslateY;
        public double LightSourceTranslateY
        {
            get => _lightSourceTranslateY;
            set => SetProperty(ref _lightSourceTranslateY, value);
        }

        private bool _isShowingSearchResults; // New flag
        private bool _isProgrammaticTreeSelection; // New flag
        private bool _preserveImageOnUpdate = false; // New flag to prevent image clearing

        private Category? _selectedCategoryInTree;
        public Category? SelectedCategoryInTree
        {
            get => _selectedCategoryInTree;
            set => SetProperty(ref _selectedCategoryInTree, value);
        }

        private SubCategory? _selectedSubCategoryInTree;
        public SubCategory? SelectedSubCategoryInTree
        {
            get => _selectedSubCategoryInTree;
            set => SetProperty(ref _selectedSubCategoryInTree, value);
        }

        // Keeping the old property for compatibility if needed elsewhere
        private Thickness _lightSourceMargin;
        public Thickness LightSourceMargin
        {
            get => _lightSourceMargin;
            set => SetProperty(ref _lightSourceMargin, value);
        }

        public void FilterItems(Axis2.WPF.Shared.SearchCriteria searchCriteria)
        {
            Logger.Log("FilterItems: Method entered.");
            DisplayedItems.Clear();
            Logger.Log("FilterItems: DisplayedItems cleared.");
            var allItems = Categories.SelectMany(c => c.SubSections.SelectMany(s => s.Items));

            IEnumerable<SObject> filteredItems = allItems;

            if (searchCriteria.IsLightSource)
            {
                filteredItems = filteredItems.Where(item =>
                {
                    _lightDataService.TileDataItems.TryGetValue(item.Id.AllToUInt(), out var tileDataItem);
                    return tileDataItem != null && (tileDataItem.Flags & 0x00800000) != 0;
                });
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.SearchTerm))
            {
                switch (searchCriteria.SelectedSearchField.DisplayName) // Use DisplayName from SearchField class
                {
                    case "Name": // Assuming "Name" maps to Description
                    case "Description":
                        filteredItems = filteredItems.Where(item =>
                            (item.Description?.Contains(searchCriteria.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                        break;
                    case "ID":
                        filteredItems = filteredItems.Where(item =>
                            (item.Id?.Contains(searchCriteria.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                        break;
                    case "Type":
                        filteredItems = filteredItems.Where(item =>
                            (item.ScriptType?.Contains(searchCriteria.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                        break;
                }
            }

            foreach (var item in filteredItems)
            {
                DisplayedItems.Add(item);
            }
            Logger.Log($"FilterItems: Added {DisplayedItems.Count} items to DisplayedItems.");
            _isShowingSearchResults = true; // Set flag after search
            Logger.Log($"FilterItems: _isShowingSearchResults set to {_isShowingSearchResults}.");
        }


    public IEnumerable<string> GetUniqueScriptTypes()
        {
            return Categories.SelectMany(c => c.SubSections.SelectMany(s => s.Items))
                             .Where(item => !string.IsNullOrEmpty(item.ScriptType))
                             .Select(item => item.ScriptType)
                             .Distinct()
                             .OrderBy(type => type);
        }

        private void ExpandAndSelectInTreeView(SObject itemToSelect)
        {
            Logger.Log($"ExpandAndSelectInTreeView: Method entered for item {itemToSelect?.Description ?? "null"}.");
            if (itemToSelect == null) return;

            // Collapse all other categories and subcategories first
            foreach (var category in Categories)
            {
                category.IsExpanded = false;
                foreach (var subCategory in category.SubSections)
                {
                    subCategory.IsExpanded = false;
                }
            }
            Logger.Log("ExpandAndSelectInTreeView: All categories/subcategories collapsed.");

            // Now, expand and select the target item's path
            foreach (var category in Categories)
            {
                foreach (var subCategory in category.SubSections)
                {
                    if (subCategory.Items.Contains(itemToSelect))
                    {
                        category.IsExpanded = true;
                        subCategory.IsExpanded = true;

                        _isProgrammaticTreeSelection = true; // Set flag before programmatic selection
                        SelectedTreeItem = subCategory; // Select the subcategory to expand it
                        _isProgrammaticTreeSelection = false; // Reset flag after programmatic selection

                        Logger.Log($"ExpandAndSelectInTreeView: Expanded category '{category.Name}' and subcategory '{subCategory.Name}'.");

                        // Set the selected path properties
                        SelectedCategoryInTree = category;
                        SelectedSubCategoryInTree = subCategory;
                        Logger.Log($"ExpandAndSelectInTreeView: SelectedCategoryInTree set to '{SelectedCategoryInTree.Name}', SelectedSubCategoryInTree set to '{SelectedSubCategoryInTree.Name}'.");
                        return;
                    }
                }
            }
            // If item not found, clear selected path properties
            SelectedCategoryInTree = null;
            SelectedSubCategoryInTree = null;
            Logger.Log("ExpandAndSelectInTreeView: Item not found in tree, clearing selected path properties.");
        }
    }
}
    
