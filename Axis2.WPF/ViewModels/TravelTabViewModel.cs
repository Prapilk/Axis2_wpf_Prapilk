using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Threading.Tasks; // Added for Task and async/await

namespace Axis2.WPF.ViewModels
{
    public class TravelTabViewModel : ViewModelBase, IHandler<ProfileLoadedEvent>
    {
        private readonly TravelDataService _travelDataService;
        private readonly MulMapService _mapService;
        private readonly SettingsService _settingsService;
        private readonly EventAggregator _eventAggregator;
        private readonly LocationService _locationService;
        private readonly IUoClient _uoClient;
        private readonly ScriptParser _scriptParser;
        private readonly ScriptParserService _scriptParserService;

        private List<TravelLocation> _allLocations;
        private List<TravelLocation> _customLocations;
        private List<RegionGroup> _allRegionGroups;
        private TravelLocation _selectedLocation;
        private MapRegion _selectedRegion;
        private RoomDefinition _selectedRoom;
        private object _selectedItem;

        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    // for compatibility, keep other properties in sync
                    SelectedRegion = _selectedItem as MapRegion;
                    SelectedRoom = _selectedItem as RoomDefinition;
                    OnPropertyChanged(nameof(CanEditDelete)); // Notify the button to update its state
                }
            }
        }
        private string _selectedCategory;
        private bool _autoRecallGate;
        private WriteableBitmap _mapImage;
        private int _zoomLevel;
        private double _mapCenterX = 3077.0;
        public double MapCenterX
        {
            get => _mapCenterX;
            set => SetProperty(ref _mapCenterX, value);
        }

        private double _mapCenterY = 2048.0;
        public double MapCenterY
        {
            get => _mapCenterY;
            set => SetProperty(ref _mapCenterY, value);
        }
        private short _selectedMapFile;
        private string _currentRegionName;
        private int _currentRegionMap;
        private int _currentRegionX;
        private int _currentRegionY;
        private int _currentRegionZ;
        private string _clickedCoordinates;
        private string _mouseMapCoordinatesText; // New property for real-time mouse coordinates
        public string MouseMapCoordinatesText
        {
            get => _mouseMapCoordinatesText;
            set => SetProperty(ref _mouseMapCoordinatesText, value);
        }
        private Visibility _crosshairVisibility = Visibility.Collapsed;
        private double _crosshairX;
        private double _crosshairY;
        private double _crosshairX_Start;
        private double _crosshairX_End;
        private double _crosshairY_End;
        private double _coordinatesTextX;
        private double _coordinatesTextY;
        private Visibility _coordinatesTextVisibility = Visibility.Collapsed;
        private double _mouseX;
        private double _mouseY;
        private double _mouseX_Start;
        private double _mouseX_End;
        private double _mouseY_End;
        private Visibility _mouseCrosshairVisibility = Visibility.Collapsed;
        private int _lastClickedMapX;
        private int _lastClickedMapY;
        private int _lastClickedMapZ;
        private short _lastClickedMapFile;
        private bool _isDragging = false;
        private bool _isDrawingMode = false;
        public bool IsDrawingMode
        {
            get => _isDrawingMode;
            set => SetProperty(ref _isDrawingMode, value);
        }

        private bool _isEditingExistingRectMode = false;
        public bool IsEditingExistingRectMode
        {
            get => _isEditingExistingRectMode;
            set => SetProperty(ref _isEditingExistingRectMode, value);
        }
        private double _drawingRectX;
        public double DrawingRectX
        {
            get => _drawingRectX;
            set => SetProperty(ref _drawingRectX, value);
        }

        private double _drawingRectY;
        public double DrawingRectY
        {
            get => _drawingRectY;
            set => SetProperty(ref _drawingRectY, value);
        }

        private double _drawingRectWidth;
        public double DrawingRectWidth
        {
            get => _drawingRectWidth;
            set => SetProperty(ref _drawingRectWidth, value);
        }

        private double _drawingRectHeight;
        public double DrawingRectHeight
        {
            get => _drawingRectHeight;
            set => SetProperty(ref _drawingRectHeight, value);
        }

        private Visibility _drawingRectangleVisibility = Visibility.Collapsed;
        public Visibility DrawingRectangleVisibility
        {
            get => _drawingRectangleVisibility;
            set => SetProperty(ref _drawingRectangleVisibility, value);
        }
        private short _selectedMap;
        private Profile _currentProfile;

        public TravelTabViewModel(TravelDataService travelDataService, MulMapService mapService, SettingsService settingsService, EventAggregator eventAggregator, LocationService locationService, IUoClient uoClient, ScriptParser scriptParser, ScriptParserService scriptParserService)
        {
            _travelDataService = travelDataService;
            _mapService = mapService;
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;
            _locationService = locationService;
            _uoClient = uoClient;
            _scriptParser = scriptParser;
            _scriptParserService = scriptParserService;

            _allLocations = new List<TravelLocation>();
            _customLocations = new List<TravelLocation>();
            Categories = new ObservableCollection<string>();
            Locations = new ObservableCollection<TravelLocation>();
            RegionGroups = new ObservableCollection<RegionGroup>();
            Rooms = new ObservableCollection<RoomDefinition>();
            AvailableMaps = new ObservableCollection<short>();

            GoCommand = new RelayCommand(Go, CanGo);
            StopCommand = new RelayCommand(Stop);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditDelete);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            ShowMapCommand = new RelayCommand(ShowMap);
            RecallCommand = new RelayCommand(Recall, CanGo);
            GateCommand = new RelayCommand(Gate, CanGo);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            LocateCommand = new RelayCommand(Locate);
            WhereCommand = new RelayCommand(Where);
            FindAreaCommand = new RelayCommand(FindArea);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            MapClickCommand = new RelayCommand<System.Windows.Point>(MapClick);
            MoveMapCommand = new RelayCommand<System.Windows.Point>(MoveMap);
            CenterMapOnCoordinatesCommand = new RelayCommand<Tuple<int, int, short, int, int>>(CenterMapOnCoordinates);
            UpdateMouseMapCoordinatesCommand = new RelayCommand<Tuple<double, double, double, double, int, int, short>>(UpdateMouseMapCoordinates);

            AddRectCommand = new RelayCommand(AddRect, CanAddEditDeleteRect);
            EditRectCommand = new RelayCommand(EditRect, CanAddEditDeleteRect);
            DeleteRectCommand = new RelayCommand(DeleteRect, CanAddEditDeleteRect);
            AutoRecallGate = true;
            ZoomLevel = 0;
            SelectedMapFile = 0; // Default to map 0

            _eventAggregator.Subscribe(this);

            LoadInitialData();
        }

        public ICommand GoCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ShowMapCommand { get; }
        public ICommand RecallCommand { get; }
        public ICommand GateCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand LocateCommand { get; }
        public ICommand WhereCommand { get; }
        public ICommand MapClickCommand { get; }
        public ICommand MoveMapCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand FindAreaCommand { get; }
        public ICommand CenterMapOnCoordinatesCommand { get; }
        public ICommand UpdateMouseMapCoordinatesCommand { get; }

        public ICommand AddRectCommand { get; }
        public ICommand EditRectCommand { get; }
        public ICommand DeleteRectCommand { get; }

        public string ClickedCoordinates
        {
            get => _clickedCoordinates;
            set => SetProperty(ref _clickedCoordinates, value);
        }

        public Visibility CrosshairVisibility
        {
            get => _crosshairVisibility;
            set => SetProperty(ref _crosshairVisibility, value);
        }

        public double CrosshairX
        {
            get => _crosshairX;
            set => SetProperty(ref _crosshairX, value);
        }

        public double CrosshairY
        {
            get => _crosshairY;
            set => SetProperty(ref _crosshairY, value);
        }

        public double CrosshairX_Start
        {
            get => _crosshairX_Start;
            set => SetProperty(ref _crosshairX_Start, value);
        }

        public double CrosshairX_End
        {
            get => _crosshairX_End;
            set => SetProperty(ref _crosshairX_End, value);
        }

        public double CrosshairY_End
        {
            get => _crosshairY_End;
            set => SetProperty(ref _crosshairY_End, value);
        }

        public double CoordinatesTextX
        {
            get => _coordinatesTextX;
            set => SetProperty(ref _coordinatesTextX, value);
        }

        public double CoordinatesTextY
        {
            get => _coordinatesTextY;
            set => SetProperty(ref _coordinatesTextY, value);
        }

        public Visibility CoordinatesTextVisibility
        {
            get => _coordinatesTextVisibility;
            set => SetProperty(ref _coordinatesTextVisibility, value);
        }

        public double MouseX
        {
            get => _mouseX;
            set => SetProperty(ref _mouseX, value);
        }

        public double MouseY
        {
            get => _mouseY;
            set => SetProperty(ref _mouseY, value);
        }

        public double MouseX_Start
        {
            get => _mouseX_Start;
            set => SetProperty(ref _mouseX_Start, value);
        }

        public double MouseX_End
        {
            get => _mouseX_End;
            set => SetProperty(ref _mouseX_End, value);
        }

        public double MouseY_End
        {
            get => _mouseY_End;
            set => SetProperty(ref _mouseY_End, value);
        }

        public Visibility MouseCrosshairVisibility
        {
            get => _mouseCrosshairVisibility;
            set => SetProperty(ref _mouseCrosshairVisibility, value);
        }

        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<TravelLocation> Locations { get; } // Keep for custom locations if needed, but not for regions
        public ObservableCollection<RegionGroup> RegionGroups { get; }
        public ObservableCollection<RoomDefinition> Rooms { get; }
        public ObservableCollection<short> AvailableMaps { get; }

        public ObservableCollection<System.Windows.Rect> SelectedRects { get; } = new ObservableCollection<System.Windows.Rect>();
        public System.Windows.Rect SelectedRect
        {
            get => _selectedRect;
            set
            {
                if (SetProperty(ref _selectedRect, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Force CanExecute re-evaluation
                }
            }
        }
        private System.Windows.Rect _selectedRect;

        private System.Windows.Rect _editingOriginalRect;
        public System.Windows.Rect EditingOriginalRect
        {
            get => _editingOriginalRect;
            set => SetProperty(ref _editingOriginalRect, value);
        }

        

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterLocationsByCategory();
                }
            }
        }

        public TravelLocation SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (SetProperty(ref _selectedLocation, value) && _selectedLocation != null)
                {
                    if (_selectedLocation.Map != this.SelectedMapFile)
                    {
                        SelectedMapFile = (short)_selectedLocation.Map;
                    }

                    _mapCenterX = (double)_selectedLocation.X;
                    _mapCenterY = (double)_selectedLocation.Y;

                    if (_selectedLocation.Map == this.SelectedMapFile)
                    {
                        LoadInitialData();
                    }
                }
            }
        }

        public RoomDefinition SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (SetProperty(ref _selectedRoom, value))
                {
                    SelectedRects.Clear(); // Clear previous rectangles
                    SelectedRect = Rect.Empty; // ADDED THIS LINE
                    if (_selectedRoom != null)
                    {
                        // *** GEMINI CHANGE START ***
                        if (_selectedRoom.Map != this.SelectedMapFile)
                        {
                            SelectedMapFile = (short)_selectedRoom.Map;
                        }
                        // *** GEMINI CHANGE END ***

                        // Update map center based on selected room's coordinates
                        _mapCenterX = _selectedRoom.P.X;
                        _mapCenterY = _selectedRoom.P.Y;
                        
                        if (_selectedRoom.Map == this.SelectedMapFile)
                        {
                            LoadInitialData(); // Recenter and redraw map
                        }

                        // Update detail properties (optional, if you want to display room details)
                        CurrentRegionName = _selectedRoom.Name; // Re-using for display
                        CurrentRegionMap = _selectedRoom.Map;
                        CurrentRegionX = (int)_selectedRoom.P.X;
                        CurrentRegionY = (int)_selectedRoom.P.Y;
                        CurrentRegionZ = _selectedRoom.Z;

                        Logger.Log($"DEBUG: Selected room '{_selectedRoom.Name}' details updated.");

                        // Populate SelectedRects with the room's rectangles
                        foreach (var rect in _selectedRoom.Rects)
                        {
                            SelectedRects.Add(rect);
                        }
                    }
                    else
                    {
                        // Clear detail properties if no room is selected
                        CurrentRegionName = string.Empty;
                        CurrentRegionMap = 0;
                        CurrentRegionX = 0;
                        CurrentRegionY = 0;
                        CurrentRegionZ = 0;
                    }
                    // No need to call DrawRegionOnMap here, as it's called by SelectedRegion or LoadInitialData
                }
            }
        }

        public MapRegion SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value))
                {
                    SelectedRects.Clear(); // Clear previous rectangles
                    SelectedRect = Rect.Empty; // ADDED THIS LINE
                    if (_selectedRegion != null)
                    {
                        // If the selected region belongs to a different map, switch to that map.
                        if (_selectedRegion.Map != this.SelectedMapFile)
                        {
                            SelectedMapFile = (short)_selectedRegion.Map;
                        }

                        // Update map center based on selected region's P or first Rect
                        if (_selectedRegion.P.X != 0 || _selectedRegion.P.Y != 0)
                        {
                            _mapCenterX = _selectedRegion.P.X;
                            _mapCenterY = _selectedRegion.P.Y;
                        }
                        else if (_selectedRegion.Rects.Any())
                        {
                            var firstRect = _selectedRegion.Rects.First();
                            _mapCenterX = firstRect.X + firstRect.Width / 2;
                            _mapCenterY = firstRect.Y + firstRect.Height / 2;
                        }
                        
                        if (_selectedRegion.Map == this.SelectedMapFile)
                        {
                            LoadInitialData(); 
                        }

                        // Update detail properties
                        CurrentRegionName = _selectedRegion.Name;
                        CurrentRegionMap = _selectedRegion.Map;
                        CurrentRegionX = (int)_selectedRegion.P.X;
                        CurrentRegionY = (int)_selectedRegion.P.Y;
                        CurrentRegionZ = _selectedRegion.Z;

                        // Populate Rooms for the newly selected region if it's an AreaDefinition
                        Rooms.Clear();
                        if (_selectedRegion is AreaDefinition areaDefinition)
                        {
                            foreach (var room in areaDefinition.Rooms)
                            {
                                Rooms.Add(room);
                            }
                        }

                        Logger.Log($"DEBUG: Selected region '{_selectedRegion.Name}' details updated.");

                        // Populate SelectedRects with the region's rectangles
                        foreach (var rect in _selectedRegion.Rects)
                        {
                            SelectedRects.Add(rect);
                        }
                    }
                    else
                    {
                        // Clear detail properties if no region is selected
                        CurrentRegionName = string.Empty;
                        CurrentRegionMap = 0;
                        CurrentRegionX = 0;
                        CurrentRegionY = 0;
                        CurrentRegionZ = 0;
                        Rooms.Clear(); // Clear rooms if no region is selected
                    }
                    DrawRegionOnMap();
                }
            }
        }

        public short SelectedMapFile
        {
            get => _selectedMap;
            set
            {
                if (SetProperty(ref _selectedMap, value))
                {
                    Logger.Log($"SelectedMapFile changed to: {value}");
                    LoadInitialData();
                }
            }
        }

        public string CurrentRegionName
        {
            get => _currentRegionName;
            set => SetProperty(ref _currentRegionName, value);
        }

        public int CurrentRegionMap
        {
            get => _currentRegionMap;
            set => SetProperty(ref _currentRegionMap, value);
        }

        public int CurrentRegionX
        {
            get => _currentRegionX;
            set => SetProperty(ref _currentRegionX, value);
        }

        public int CurrentRegionY
        {
            get => _currentRegionY;
            set => SetProperty(ref _currentRegionY, value);
        }

        public int CurrentRegionZ
        {
            get => _currentRegionZ;
            set => SetProperty(ref _currentRegionZ, value);
        }

        public bool AutoRecallGate
        {
            get => _autoRecallGate;
            set => SetProperty(ref _autoRecallGate, value);
        }

        public WriteableBitmap MapImage
        {
            get => _mapImage;
            set => SetProperty(ref _mapImage, value);
        }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (value < -3) value = -3;
                if (value > 3) value = 3;

                if (SetProperty(ref _zoomLevel, value))
                {
                    LoadInitialData();
                }
            }
        }

        public void Handle(ProfileLoadedEvent e)
        {
            Logger.Log("--- TravelTabViewModel: OnProfileLoaded event received ---");
            if (e?.LoadedProfile == null)
            {
                Logger.Log("ERROR: OnProfileLoaded received a null profile.");
                return;
            }
            Logger.Log($"DEBUG: Profile name: {e.LoadedProfile.Name}");

            LoadInitialData(); // Reload map with new paths
            LoadTravelData(e.LoadedProfile);
        }

        private void LoadInitialData(int? overrideViewPortWidth = null, int? overrideViewPortHeight = null)
        {
            Logger.Log($"--- LoadInitialData START for map index: {SelectedMapFile} ---");
            var settings = _settingsService.LoadSettings();
            
            var mapPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == $"map{SelectedMapFile}.mul")?.FilePath;
            var staidxPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == $"staidx{SelectedMapFile}.mul")?.FilePath;
            var staticsPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == $"statics{SelectedMapFile}.mul")?.FilePath;
            var radarcolPath = settings.OverridePathsSettings.FilePaths.FirstOrDefault(p => p.FileName == "radarcol.mul")?.FilePath;

            Logger.Log($"Path for map{SelectedMapFile}.mul: {mapPath ?? "NOT FOUND"}");
            Logger.Log($"Path for staidx{SelectedMapFile}.mul: {staidxPath ?? "NOT FOUND"}");
            Logger.Log($"Path for statics{SelectedMapFile}.mul: {staticsPath ?? "NOT FOUND"}");

            StaticsService currentStaticsService = null;
            if (!string.IsNullOrEmpty(staidxPath) && !string.IsNullOrEmpty(staticsPath) && !string.IsNullOrEmpty(radarcolPath))
            {
                currentStaticsService = new StaticsService(staidxPath, staticsPath, radarcolPath);
                Logger.Log("StaticsService created successfully.");
            }
            else
            {
                Logger.Log($"ERROR: Could not create StaticsService because one or more paths were missing for map {SelectedMapFile}.");
            }

            if (!AvailableMaps.Any())
            {
                for (short i = 0; i <= 5; i++) 
                {
                    AvailableMaps.Add(i);
                }
            }

            if (!string.IsNullOrEmpty(mapPath) && currentStaticsService != null)
            {
                int actualViewPortWidth = overrideViewPortWidth ?? 1024; 
                int actualViewPortHeight = overrideViewPortHeight ?? 768;

                Logger.Log($"Calling RenderMap with: mapIndex={SelectedMapFile}, mapPath='{mapPath}', zoom={ZoomLevel}, center=({_mapCenterX},{_mapCenterY}), viewport={actualViewPortWidth}x{actualViewPortHeight}");
                MapImage = _mapService.RenderMap(SelectedMapFile, mapPath, actualViewPortWidth, actualViewPortHeight, ZoomLevel, (int)_mapCenterX, (int)_mapCenterY, currentStaticsService);
                
                if (MapImage != null)
                {
                    Logger.Log($"RenderMap returned a WriteableBitmap of size {MapImage.PixelWidth}x{MapImage.PixelHeight}.");
                    ClickedCoordinates = $"X: {MapCenterX}, Y: {MapCenterY}, Map: {SelectedMapFile}";
                    CrosshairVisibility = Visibility.Visible;
                }
                else
                {
                    Logger.Log("ERROR: RenderMap returned null.");
                }

                DrawRegionOnMap();
            }
            else
            {
                Logger.Log("ERROR: Rendering skipped because mapPath is missing or StaticsService could not be created.");
                MapImage = null;
            }
            Logger.Log("--- LoadInitialData END ---");
        }

        private async void LoadTravelData(Profile profile)
        {
            Logger.Log("DEBUG: LoadTravelData method entered.");
            if (profile == null || string.IsNullOrEmpty(profile.BaseDirectory) || !profile.SelectedScripts.Any())
            {
                Logger.Log("TravelTabViewModel: Profile, BaseDirectory, or SelectedScripts are null or empty. Clearing travel data.");
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RegionGroups.Clear();
                    _allLocations.Clear();
                    Locations.Clear();
                    Categories.Clear();
                });
                return;
            }

            _currentProfile = profile;
            var mapScriptPaths = profile.SelectedScripts.Select(s => s.Path).Distinct().ToList();
            Logger.Log($"DEBUG: TravelTabViewModel - Found {mapScriptPaths.Count} unique scripts to parse.");

            var allParsedObjects = new List<SObject>();
            await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var scriptPath in mapScriptPaths)
                {
                    if (File.Exists(scriptPath))
                    {
                        allParsedObjects.AddRange(_scriptParser.ParseFile(scriptPath));
                    }
                }
            });

            var travelObjects = allParsedObjects.Where(o => o.Region != null).ToList();
            Logger.Log($"DEBUG: Found {travelObjects.Count} travel-related SObjects after parsing.");

            var groups = travelObjects.GroupBy(o => o.Region.Group).ToDictionary(g => g.Key, g => g.ToList());
            Logger.Log($"DEBUG: Grouped into {groups.Count} region groups.");

            var newRegionGroups = new List<RegionGroup>();
            foreach (var group in groups.OrderBy(g => g.Key)) // Sort groups alphabetically
            {
                var regionGroup = new RegionGroup { Name = group.Key, IsExpanded = false };
                var areas = group.Value.Where(o => o.Type == SObjectType.Area).Select(o => o.Region as AreaDefinition).Where(a => a != null).Distinct().ToList();

                var topLevelRooms = group.Value.Where(o =>
                    {
                        if (o.Type != SObjectType.Room) return false;
                        var room = o.Region as RoomDefinition;
                        if (room == null) return false;
                        return !areas.Any(a => a.Rooms.Any(r => r.Name == room.Name && r.Group == room.Group && r.Map == room.Map));
                    })
                    .Select(o => o.Region as RoomDefinition)
                    .Where(r => r != null)
                    .ToList();

                // Sort areas and rooms before adding them
                foreach (var area in areas.OrderBy(a => a.Name))
                {
                    // Assuming AreaDefinition.Rooms is a List<T>, sort it in place.
                    area.Rooms.Sort((r1, r2) => string.Compare(r1.Name, r2.Name, StringComparison.Ordinal));
                    regionGroup.Areas.Add(area);
                }

                regionGroup.Rooms.Sort((r1, r2) => string.Compare(r1.Name, r2.Name, StringComparison.Ordinal));

                if (regionGroup.Areas.Any() || regionGroup.Rooms.Any())
                {
                    newRegionGroups.Add(regionGroup);
                }
            }

            var newAllLocations = new List<TravelLocation>();
            foreach (var sObject in travelObjects.OrderBy(o => o.Region.Name)) // Sort locations alphabetically
            {
                var region = sObject.Region;
                int locX = 0, locY = 0;
                if (region.P.X != 0 || region.P.Y != 0)
                {
                    locX = (int)region.P.X;
                    locY = (int)region.P.Y;
                }
                else if (region.Rects.Any())
                {
                    var firstRect = region.Rects.First();
                    locX = (int)(firstRect.X + firstRect.Width / 2);
                    locY = (int)(firstRect.Y + firstRect.Height / 2);
                }
                newAllLocations.Add(new TravelLocation { Name = region.Name, Category = region.Group, Map = region.Map, X = locX, Y = locY, Z = region.Z });
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RegionGroups.Clear();
                foreach (var group in newRegionGroups) // Already sorted from the OrderBy on the dictionary keys
                {
                    RegionGroups.Add(group);
                }
                Logger.Log($"TravelTabViewModel: Finished UI update. Loaded {RegionGroups.Count} region groups into the final collection.");

                _allLocations = newAllLocations;
                _customLocations = _locationService.LoadCustomLocations();
                _allLocations.AddRange(_customLocations);

                Categories.Clear();
                var allCategories = _allLocations.Select(l => l.Category).Distinct().OrderBy(c => c);
                foreach (var category in allCategories)
                {
                    Categories.Add(category);
                }

                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
                else
                {
                    Locations.Clear();
                }
                Logger.Log($"TravelTabViewModel: Finished populating locations. Total categories: {Categories.Count}");
            });
        }

        

        private void FilterLocationsByCategory()
        {
            Locations.Clear();
            Logger.Log($"DEBUG: FilterLocationsByCategory entered. SelectedCategory: {SelectedCategory ?? "(null)"}");

            if (SelectedCategory != null)
            {
                var filtered = _allLocations.Where(l => l.Category == SelectedCategory);
                foreach (var location in filtered)
                {
                    Locations.Add(location);
                }
                Logger.Log($"DEBUG: FilterLocationsByCategory - Found {Locations.Count} locations for category '{SelectedCategory}'.");
            }
        }

        private bool CanGo() => SelectedRoom != null || (_lastClickedMapX != 0 || _lastClickedMapY != 0);

        private bool CanEditDelete() => SelectedItem != null && !SelectedRect.IsEmpty;

        private bool CanDelete() => SelectedItem != null;

        private void Go()
        {
            string command = string.Empty;
            string prefix = _settingsService.LoadSettings().GeneralSettings.CommandPrefix;

            if (SelectedRoom != null)
            {
                command = $"go {SelectedRoom.P.X},{SelectedRoom.P.Y},{SelectedRoom.Z},{SelectedRoom.Map}";
            }
            else if (_lastClickedMapX != 0 || _lastClickedMapY != 0)
            {
                command = $"go {_lastClickedMapX},{_lastClickedMapY},{_lastClickedMapZ},{_lastClickedMapFile}";
            }

            if (!string.IsNullOrEmpty(command))
            {
                _uoClient.SendToClient(command);
            }
        }

        private void Stop() => _uoClient.SendToClient("stop");

        private void Add()
        {
            IsDrawingMode = true;
            IsEditingExistingRectMode = false; // Ensure editing mode is off
            System.Windows.MessageBox.Show("Draw the new room/area on the map.", "Drawing Mode", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void Edit()
        {
            if (SelectedRect.IsEmpty)
            {
                System.Windows.MessageBox.Show("Please select a rectangle in the list to edit.", "No Rectangle Selected", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is not (RoomDefinition or AreaDefinition))
            {
                System.Windows.MessageBox.Show("Please select a room or an area to which the rectangle belongs.", "No Room or Area Selected", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            // Store the original rectangle for editing
            EditingOriginalRect = SelectedRect;
            
            // Activate drawing mode for editing
            IsDrawingMode = true;
            IsEditingExistingRectMode = true; // ADDED THIS LINE
            System.Windows.MessageBox.Show("Redraw the new area for the selected rectangle on the map.", "Redraw for Edit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private bool CanAddEditDeleteRect() => SelectedRoom != null || SelectedRegion != null;

        private void AddRect()
        {
            IsDrawingMode = true;
            System.Windows.MessageBox.Show("Draw the new rectangle on the map.", "Drawing Mode", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void EditRect()
        {
            if (SelectedRect != null)
            {
                var viewModel = new Travel.AddEditRectViewModel();
                viewModel.FromRect(SelectedRect);

                var window = new Views.Travel.AddEditRectWindow() { DataContext = viewModel };
                window.Owner = System.Windows.Application.Current.MainWindow;

                if (window.ShowDialog() == true)
                {
                    // Update the selected rect
                    SelectedRect = viewModel.ToRect();
                    // You might need to trigger a UI update for the map here
                    DrawRegionOnMap();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a rectangle to edit.", "No Selection", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void DeleteRect()
        {
            if (SelectedRect != null)
            {
                if (System.Windows.MessageBox.Show("Are you sure you want to delete this rectangle?", "Confirm Delete", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                {
                    SelectedRects.Remove(SelectedRect);
                    // You might need to trigger a UI update for the map here
                    DrawRegionOnMap();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a rectangle to delete.", "No Selection", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void Delete()
        {
            if (SelectedItem == null) return;

            // Case 1: The selected item is a RoomDefinition.
            if (SelectedItem is RoomDefinition roomToDelete)
            {
                if (System.Windows.MessageBox.Show($"Are you sure you want to delete Room: {roomToDelete.Name}?", "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                {
                    bool removed = false;
                    foreach (var group in RegionGroups)
                    {
                        foreach (var area in group.Areas)
                        {
                            if (area.Rooms.Remove(roomToDelete)) { removed = true; break; }
                        }
                        if (removed) break;
                        if (group.Rooms.Remove(roomToDelete)) break;
                    }

                    if (_currentProfile != null && _currentProfile.SelectedScripts.Any())
                    {
                        _scriptParserService.SaveMapScript(RegionGroups, _currentProfile.SelectedScripts.First().Path);
                        LoadTravelData(_currentProfile);
                    }
                }
            }
            // Case 2: The selected item is an AreaDefinition.
            else if (SelectedItem is AreaDefinition areaToDelete)
            {
                if (System.Windows.MessageBox.Show($"Are you sure you want to delete Area: {areaToDelete.Name}? This will delete all rooms inside it.", "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                {
                    bool removed = false;
                    foreach (var group in RegionGroups)
                    {
                        if (group.Areas.Remove(areaToDelete)) { removed = true; break; }
                    }

                    if (removed && _currentProfile != null && _currentProfile.SelectedScripts.Any())
                    {
                        _scriptParserService.SaveMapScript(RegionGroups, _currentProfile.SelectedScripts.First().Path);
                        LoadTravelData(_currentProfile);
                    }
                }
            }
            // Case 3: The selected item is a RegionGroup.
            else if (SelectedItem is RegionGroup groupToDelete)
            {
                if (System.Windows.MessageBox.Show($"Are you sure you want to delete the entire Group: {groupToDelete.Name}?", "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                {
                    RegionGroups.Remove(groupToDelete);

                    if (_currentProfile != null && _currentProfile.SelectedScripts.Any())
                    {
                        _scriptParserService.SaveMapScript(RegionGroups, _currentProfile.SelectedScripts.First().Path);
                        LoadTravelData(_currentProfile);
                    }
                }
            }
        }

        private void ShowMap()
        {
            var window = new Axis2.WPF.Travel.WorldMapView();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.Show();
        }

        private void Recall()
        {
            if (SelectedLocation != null)
            {
                string command = $"{_settingsService.LoadSettings().GeneralSettings.CommandPrefix}recall {SelectedLocation.X},{SelectedLocation.Y},{SelectedLocation.Z},{SelectedLocation.Map}";
                _uoClient.SendToClient(command);
            }
        }

        private void Gate()
        {
            if (SelectedLocation != null)
            {
                string command = $"{_settingsService.LoadSettings().GeneralSettings.CommandPrefix}gate {SelectedLocation.X},{SelectedLocation.Y},{SelectedLocation.Z},{SelectedLocation.Map}";
                _uoClient.SendToClient(command);
            }
        }

        private void ZoomIn() => ZoomLevel++;

        private void ZoomOut() => ZoomLevel--;

        private void Locate() { }

        private void Where() => _uoClient.SendToClient("where");

        private void FindArea()
        {
            var allRooms = RegionGroups.SelectMany(g => g.Areas.SelectMany(a => a.Rooms)).Concat(RegionGroups.SelectMany(g => g.Rooms));
            var findViewModel = new Travel.FindAreaViewModel(allRooms);
            var findWindow = new Axis2.WPF.Travel.FindArea(findViewModel);
            findWindow.Owner = System.Windows.Application.Current.MainWindow;

            if (findWindow.ShowDialog() == true && findViewModel.SelectedRoom != null)
            {
                SelectedRoom = findViewModel.SelectedRoom;
                SelectRoomInTree(findViewModel.SelectedRoom);
            }
        }

        private void ClearSearch()
        {
            Rooms.Clear();
            var allRooms = RegionGroups.SelectMany(g => g.Areas.SelectMany(a => a.Rooms)).Concat(RegionGroups.SelectMany(g => g.Rooms));
            foreach (var room in allRooms.Distinct())
            {
                Rooms.Add(room);
            }
        }

        private void SelectRoomInTree(RoomDefinition roomToSelect)
        {
            foreach (var group in RegionGroups)
            {
                if (group.Rooms.Contains(roomToSelect))
                {
                    group.IsExpanded = true;
                    SelectedRegion = roomToSelect;
                    return;
                }

                foreach (var area in group.Areas)
                {
                    if (area.Rooms.Contains(roomToSelect))
                    {
                        group.IsExpanded = true;
                        area.IsExpanded = true;
                        SelectedRegion = area;
                        SelectedRoom = roomToSelect;
                        return;
                    }
                }
            }
        }

        private void MoveMap(System.Windows.Point delta)
        {
            double currentScaleFactor;
            if (ZoomLevel >= 0)
            {
                currentScaleFactor = Math.Pow(2, ZoomLevel);
            }
            else
            {
                currentScaleFactor = 1.0 / Math.Pow(2, Math.Abs(ZoomLevel));
            }

            _mapCenterX -= delta.X / currentScaleFactor;
            _mapCenterY -= delta.Y / currentScaleFactor;

            LoadInitialData();
        }

        private void MapClick(System.Windows.Point clickPoint)
        {
            // This method is now redundant. The logic has been moved to 
            // MapOverlayCanvas_MouseDown in the view, which calls 
            // CenterMapOnCoordinatesCommand. This method can be left empty
            // or removed if MapClickCommand is no longer used.
        }

        private void CenterMapOnCoordinates(Tuple<int, int, short, int, int> parameters)
        {
            int mapX = parameters.Item1;
            int mapY = parameters.Item2;
            short mapFile = parameters.Item3;
            int viewPortWidth = parameters.Item4;
            int viewPortHeight = parameters.Item5;

            bool mapFileChanged = SelectedMapFile != mapFile;

            MapCenterX = mapX;
            MapCenterY = mapY;
            SelectedMapFile = mapFile; // This will trigger LoadInitialData() if mapFileChanged is true

            // Update _lastClickedMapX/Y to the new center coordinates
            _lastClickedMapX = mapX;
            _lastClickedMapY = mapY;
            _lastClickedMapZ = 0; // Assuming Z is 0 for now, as it's not passed
            _lastClickedMapFile = mapFile;

            if (!mapFileChanged)
            {
                LoadInitialData(viewPortWidth, viewPortHeight); // Only call if SelectedMapFile didn't trigger it
            }
        }

        private void UpdateMouseMapCoordinates(Tuple<double, double, double, double, int, int, short> parameters)
        {
            double mouseX_bitmap = parameters.Item1;
            double mouseY_bitmap = parameters.Item2;
            double bitmapWidth = parameters.Item3;
            double bitmapHeight = parameters.Item4;
            int currentMapCenterX = parameters.Item5;
            int currentMapCenterY = parameters.Item6;
            int zoomLevel = parameters.Item7; // Corrected from short to int

            double currentScaleFactor;
            if (zoomLevel >= 0)
            {
                currentScaleFactor = Math.Pow(2, zoomLevel);
            }
            else
            {
                currentScaleFactor = 1.0 / Math.Pow(2, Math.Abs(zoomLevel));
            }

            double viewPortOriginX_map = (double)currentMapCenterX - (bitmapWidth / 2.0) / currentScaleFactor;
            double viewPortOriginY_map = (double)currentMapCenterY - (bitmapHeight / 2.0) / currentScaleFactor;

            int mouseMapX = (int)Math.Round(viewPortOriginX_map + mouseX_bitmap / currentScaleFactor);
            int mouseMapY = (int)Math.Round(viewPortOriginY_map + mouseY_bitmap / currentScaleFactor);

            MouseMapCoordinatesText = $"X: {mouseMapX}, Y: {mouseMapY}";
        }

                private void DrawRegionOnMap()
        {
            if (MapImage == null) return;

            try
            {
                MapImage.Lock();
                double currentScaleFactor;
                if (ZoomLevel >= 0)
                {
                    currentScaleFactor = Math.Pow(2, ZoomLevel);
                }
                else
                {
                    currentScaleFactor = 1.0 / Math.Pow(2, Math.Abs(ZoomLevel));
                }

                double viewPortOriginX_map = _mapCenterX - (MapImage.PixelWidth / 2.0) / currentScaleFactor;
                double viewPortOriginY_map = _mapCenterY - (MapImage.PixelHeight / 2.0) / currentScaleFactor;

                // Always draw the selected region's perimeter (red) if a region is selected
                if (SelectedRegion != null)
                {
                    foreach (var rect in SelectedRegion.Rects)
                    {
                        int rectDrawX = (int)((rect.X - viewPortOriginX_map) * currentScaleFactor);
                        int rectDrawY = (int)((rect.Y - viewPortOriginY_map) * currentScaleFactor);
                        int rectWidth = (int)(rect.Width * currentScaleFactor);
                        int rectHeight = (int)(rect.Height * currentScaleFactor);

                        // Calculate the intersection of the rect with the MapImage bounds
                        int clippedX1 = Math.Max(0, rectDrawX);
                        int clippedY1 = Math.Max(0, rectDrawY);
                        int clippedX2 = Math.Min(MapImage.PixelWidth, rectDrawX + rectWidth);
                        int clippedY2 = Math.Min(MapImage.PixelHeight, rectDrawY + rectHeight);

                        int finalDrawX = clippedX1;
                        int finalDrawY = clippedY1;
                        int finalWidth = clippedX2 - clippedX1;
                        int finalHeight = clippedY2 - clippedY1;

                        // Ensure dimensions are at least 1 if the original rect had positive dimensions
                        if (rectWidth > 0 && finalWidth == 0) finalWidth = 1;
                        if (rectHeight > 0 && finalHeight == 0) finalHeight = 1;

                        if (finalWidth <= 0 || finalHeight <= 0)
                        {
                            continue; // Nothing to draw
                        }

                        byte red = 255;
                        byte green = 0;
                        byte blue = 0;
                        byte alpha = 255; // Fully opaque
                        byte[] singlePixelColor = { blue, green, red, alpha }; // BGRA (4 bytes)

                        int thickness = 2; // Modifier ici pour changer l'épaisseur

                        // Dessiner la bordure supérieure
                        for (int dy = 0; dy < thickness; dy++)
                        {
                            int yOffset = finalDrawY + dy;
                            if (yOffset >= 0 && yOffset < MapImage.PixelHeight)
                            {
                                byte[] topBorderPixels = new byte[finalWidth * 4]; // 4 bytes per pixel
                                for (int i = 0; i < finalWidth * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, topBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(finalDrawX, yOffset, finalWidth, 1), topBorderPixels, finalWidth * 4, 0);
                            }
                        }

                        // Dessiner la bordure inférieure
                        for (int dy = 0; dy < thickness; dy++)
                        {
                            int yOffset = finalDrawY + finalHeight - 1 - dy;
                            if (yOffset >= 0 && yOffset < MapImage.PixelHeight)
                            {
                                byte[] bottomBorderPixels = new byte[finalWidth * 4]; // 4 bytes per pixel
                                for (int i = 0; i < finalWidth * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, bottomBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(finalDrawX, yOffset, finalWidth, 1), bottomBorderPixels, finalWidth * 4, 0);
                            }
                        }

                        // Dessiner la bordure gauche
                        for (int dx = 0; dx < thickness; dx++)
                        {
                            int xOffset = finalDrawX + dx;
                            if (xOffset >= 0 && xOffset < MapImage.PixelWidth)
                            {
                                byte[] leftBorderPixels = new byte[finalHeight * 4]; // 4 bytes per pixel
                                for (int i = 0; i < finalHeight * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, leftBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(xOffset, finalDrawY, 1, finalHeight), leftBorderPixels, 4, 0);
                            }
                        }

                        // Dessiner la bordure droite
                        for (int dx = 0; dx < thickness; dx++)
                        {
                            int xOffset = finalDrawX + finalWidth - 1 - dx;
                            if (xOffset >= 0 && xOffset < MapImage.PixelWidth)
                            {
                                byte[] rightBorderPixels = new byte[finalHeight * 4];
                                for (int i = 0; i < finalHeight * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, rightBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(xOffset, finalDrawY, 1, finalHeight), rightBorderPixels, 4, 0);
                            }
                        }
                    }
                }

                // Draw the selected room's perimeter (yellow) if a room is selected
                if (_selectedRoom != null)
                {
                    int pointDrawX = (int)((_selectedRoom.P.X - viewPortOriginX_map) * currentScaleFactor);
                    int pointDrawY = (int)((_selectedRoom.P.Y - viewPortOriginY_map) * currentScaleFactor);

                    int squareSize = 5; // Size of the square (e.g., 5x5 pixels)

                    // Calculate the intersection of the square with the MapImage bounds
                    int clippedX1 = Math.Max(0, pointDrawX - squareSize / 2);
                    int clippedY1 = Math.Max(0, pointDrawY - squareSize / 2);
                    int clippedX2 = Math.Min(MapImage.PixelWidth, pointDrawX + squareSize / 2);
                    int clippedY2 = Math.Min(MapImage.PixelHeight, pointDrawY + squareSize / 2);

                    int finalDrawX = clippedX1;
                    int finalDrawY = clippedY1;
                    int finalWidth = clippedX2 - clippedX1;
                    int finalHeight = clippedY2 - clippedY1;

                    if (finalWidth <= 0 || finalHeight <= 0)
                    {
                        // Nothing to draw for this point
                    }
                    else
                    {
                        byte yellowRed = 255;
                        byte yellowGreen = 255;
                        byte yellowBlue = 0;
                        byte yellowAlpha = 255; // Fully opaque
                        byte[] singlePixelColor = { yellowBlue, yellowGreen, yellowRed, yellowAlpha }; // BGRA (4 bytes)

                        int thickness = 2; // Thickness of the square border

                        // Dessiner la bordure supérieure
                        for (int dy = 0; dy < thickness; dy++)
                        {
                            int yOffset = finalDrawY + dy;
                            if (yOffset >= 0 && yOffset < MapImage.PixelHeight)
                            {
                                byte[] topBorderPixels = new byte[finalWidth * 4];
                                for (int i = 0; i < finalWidth * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, topBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(finalDrawX, yOffset, finalWidth, 1), topBorderPixels, finalWidth * 4, 0);
                            }
                        }

                        // Dessiner la bordure inférieure
                        for (int dy = 0; dy < thickness; dy++)
                        {
                            int yOffset = finalDrawY + finalHeight - 1 - dy;
                            if (yOffset >= 0 && yOffset < MapImage.PixelHeight)
                            {
                                byte[] bottomBorderPixels = new byte[finalWidth * 4];
                                for (int i = 0; i < finalWidth * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, bottomBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(finalDrawX, yOffset, finalWidth, 1), bottomBorderPixels, finalWidth * 4, 0);
                            }
                        }

                        // Dessiner la bordure gauche
                        for (int dx = 0; dx < thickness; dx++)
                        {
                            int xOffset = finalDrawX + dx;
                            if (xOffset >= 0 && xOffset < MapImage.PixelWidth)
                            {
                                byte[] leftBorderPixels = new byte[finalHeight * 4];
                                for (int i = 0; i < finalHeight * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, leftBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(xOffset, finalDrawY, 1, finalHeight), leftBorderPixels, 4, 0);
                            }
                        }

                        // Dessiner la bordure droite
                        for (int dx = 0; dx < thickness; dx++)
                        {
                            int xOffset = finalDrawX + finalWidth - 1 - dx;
                            if (xOffset >= 0 && xOffset < MapImage.PixelWidth)
                            {
                                byte[] rightBorderPixels = new byte[finalHeight * 4];
                                for (int i = 0; i < finalHeight * 4; i += 4)
                                {
                                    Buffer.BlockCopy(singlePixelColor, 0, rightBorderPixels, i, 4);
                                }
                                MapImage.WritePixels(new Int32Rect(xOffset, finalDrawY, 1, finalHeight), rightBorderPixels, 4, 0);
                            }
                        }
                    }
                }
            }
            finally { MapImage.Unlock(); }
        }

                public void ProcessDrawnRectangle(System.Windows.Rect drawnRect)
        {
            // Pass RegionGroups directly instead of sortedGroups
            var viewModel = new Travel.AddEditRoomViewModel(RegionGroups, AvailableMaps, drawnRect);
            var window = new Views.Travel.AddEditRoomWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;

            if (window.ShowDialog() == true)
            {
                // If the user typed a name in the "New Group" box, they are creating an Area.
                if (!string.IsNullOrEmpty(viewModel.NewGroupName))
                {
                    // --- CREATE NEW AREA ---
                    var newArea = new AreaDefinition
                    {
                        Name = viewModel.Name, // The main name for the AREADEF
                        Group = viewModel.NewGroupName, // The group the new area belongs to
                        Map = viewModel.SelectedMap,
                        P = new System.Windows.Point(viewModel.X, viewModel.Y),
                        Z = viewModel.Z
                    };
                    newArea.Rects.Add(drawnRect);

                    // Find the parent RegionGroup (e.g., "Jails") or create it if it doesn't exist.
                    var parentGroup = RegionGroups.FirstOrDefault(g => g.Name == newArea.Group);
                    if (parentGroup == null)
                    {
                        parentGroup = new RegionGroup { Name = newArea.Group };
                        RegionGroups.Add(parentGroup);
                    }
                    parentGroup.Areas.Add(newArea);
                }
                else
                {
                    // --- CREATE NEW ROOM ---
                    var newRoom = viewModel.ToRoomDefinition();

                    // Find the parent AreaDefinition (selected from the dropdown) to add the new room to.
                    // Use viewModel.SelectedArea instead of searching by name
                    var parentArea = viewModel.SelectedArea; 
                    if (parentArea != null)
                    {
                        parentArea.Rooms.Add(newRoom);
                    }
                    else
                    {
                        // If no area is selected, add to the selected RegionGroup's top-level rooms
                        if (viewModel.SelectedRegionGroup != null)
                        {
                            viewModel.SelectedRegionGroup.Rooms.Add(newRoom);
                        }
                        else
                        {
                            Logger.Log($"Warning: Could not find parent area or group for new room '{newRoom.Name}'. Room not added.");
                            return; // Exit without saving
                        }
                    }
                }

                // Save the script file with the new data.
                if (_currentProfile != null && _currentProfile.SelectedScripts.Any())
                {
                    _scriptParserService.SaveMapScript(RegionGroups, _currentProfile.SelectedScripts.First().Path);
                }

                // Reload the data to ensure the TreeView is perfectly up-to-date.
                LoadTravelData(_currentProfile);
            }
            else
            {
                Logger.Log("DEBUG: Drawn Rectangle Cancelled.");
            }
        }

        public void ProcessEditedRectangle(System.Windows.Rect newDrawnRect)
        {
            // Use EditingOriginalRect as the original rectangle
            System.Windows.Rect originalRect = EditingOriginalRect;

            // Determine the original name
            string originalName = string.Empty;
            MapRegion editedRegion = null; // To hold the actual RoomDefinition or AreaDefinition being edited

            if (SelectedRoom != null && SelectedRoom.Rects.Contains(originalRect))
            {
                originalName = SelectedRoom.Name;
                editedRegion = SelectedRoom;
            }
            else if (SelectedRegion != null)
            {
                if (SelectedRegion.Rects.Contains(originalRect))
                {
                    originalName = SelectedRegion.Name;
                    editedRegion = SelectedRegion;
                }
                else if (SelectedRegion is AreaDefinition areaDef)
                {
                    var room = areaDef.Rooms.FirstOrDefault(r => r.Rects.Contains(originalRect));
                    if (room != null)
                    {
                        originalName = room.Name;
                        editedRegion = room;
                    }
                }
            }

            // Open the AddEditRectWindow pre-filled with the new drawn coordinates and original name
            var viewModel = new Travel.AddEditRectViewModel();
            viewModel.FromRect(newDrawnRect);
            viewModel.Name = originalName; // Pass the original name

            var window = new Views.Travel.AddEditRectWindow() { DataContext = viewModel };
            window.Owner = System.Windows.Application.Current.MainWindow;

            if (window.ShowDialog() == true)
            {
                var newRect = viewModel.ToRect();
                string newName = viewModel.Name; // Get the potentially changed name

                System.Collections.Generic.IList<Rect> sourceRects = null;
                // Find the source of the original rectangle
                if (SelectedRoom != null && SelectedRoom.Rects.Contains(originalRect))
                {
                    sourceRects = SelectedRoom.Rects;
                }
                else if (SelectedRegion != null)
                {
                    if (SelectedRegion.Rects.Contains(originalRect))
                    {
                        sourceRects = SelectedRegion.Rects;
                    }
                    else if (SelectedRegion is AreaDefinition areaDef)
                    {
                        var room = areaDef.Rooms.FirstOrDefault(r => r.Rects.Contains(originalRect));
                        if (room != null)
                        {
                            sourceRects = room.Rects;
                        }
                    }
                }

                if (sourceRects != null)
                {
                    int index = sourceRects.IndexOf(originalRect);
                    if (index != -1)
                    {
                        sourceRects[index] = newRect;

                        // Update the name if it has changed
                        if (editedRegion != null && editedRegion.Name != newName)
                        {
                            editedRegion.Name = newName;
                        }

                        // Update the UI collection (SelectedRects)
                        int displayIndex = SelectedRects.IndexOf(originalRect);
                        if (displayIndex != -1)
                        {
                            SelectedRects[displayIndex] = newRect;
                        }

                        // Save changes to the script file
                        if (_currentProfile != null && _currentProfile.SelectedScripts.Any())
                        {
                            _scriptParserService.SaveMapScript(RegionGroups, _currentProfile.SelectedScripts.First().Path);
                            Logger.Log($"DEBUG: Saved edited rectangle and name for room/area.");
                        }
                        DrawRegionOnMap(); // Redraw map to show changes
                    }
                    else
                    {
                        Logger.Log("WARNING: ProcessEditedRectangle: Could not find the original rectangle in sourceRects.");
                    }
                }
                else
                {
                    Logger.Log("WARNING: ProcessEditedRectangle: Could not determine sourceRects for the edited rectangle.");
                }
            }
        }

        
    }
}