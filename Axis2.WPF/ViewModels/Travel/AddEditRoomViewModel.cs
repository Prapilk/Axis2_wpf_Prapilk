using System.Collections.ObjectModel;
using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using System.Windows;
using System.ComponentModel; // Added for IDataErrorInfo
using System.Linq; // Added for LINQ

namespace Axis2.WPF.ViewModels.Travel
{
    public class AddEditRoomViewModel : ViewModelBase, IDataErrorInfo // Implemented IDataErrorInfo
    {
        private string _name;
        // private string _selectedGroup; // No longer needed as a string
        private short _selectedMap;
        private int _x;
        private int _y;
        private int _z;
        private int _x1;
        private int _y1;
        private int _x2;
        private int _y2;
        private int _width;
        private int _height;
        private string _newGroupName;

        private RegionGroup _selectedRegionGroup; // New property for selected RegionGroup
        private AreaDefinition _selectedArea; // New property for selected Area

        public AddEditRoomViewModel(ObservableCollection<RegionGroup> regionGroups, ObservableCollection<short> availableMaps, System.Windows.Rect? rect = null)
        {
            RegionGroups = regionGroups;
            AvailableMaps = availableMaps;
            AreasInSelectedGroup = new ObservableCollection<AreaDefinition>(); // Initialize

            if (rect.HasValue)
            {
                X1 = (int)rect.Value.X;
                Y1 = (int)rect.Value.Y;
                Width = (int)rect.Value.Width;
                Height = (int)rect.Value.Height;
                X2 = (int)rect.Value.Right;
                Y2 = (int)rect.Value.Bottom;

                // Calculate center X and Y for the P property
                X = (int)(rect.Value.X + rect.Value.Width / 2);
                Y = (int)(rect.Value.Y + rect.Value.Height / 2);
                Z = 0; // Default Z to 0, as it's not derived from the drawn rectangle
            }
        }

        public string NewGroupName
        {
            get => _newGroupName;
            set => SetProperty(ref _newGroupName, value);
        }

        public int X1
        {
            get => _x1;
            set => SetProperty(ref _x1, value);
        }

        public int Y1
        {
            get => _y1;
            set => SetProperty(ref _y1, value);
        }

        public int X2
        {
            get => _x2;
            set => SetProperty(ref _x2, value);
        }

        public int Y2
        {
            get => _y2;
            set => SetProperty(ref _y2, value);
        }

        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(IsValid)); // Notify that IsValid has changed
                }
            }
        }

        public ObservableCollection<RegionGroup> RegionGroups { get; } // Renamed from Groups
        public ObservableCollection<AreaDefinition> AreasInSelectedGroup { get; } // New collection for areas
        public ObservableCollection<short> AvailableMaps { get; }

        public RegionGroup SelectedRegionGroup // New property
        {
            get => _selectedRegionGroup;
            set
            {
                if (SetProperty(ref _selectedRegionGroup, value))
                {
                    AreasInSelectedGroup.Clear();
                    if (_selectedRegionGroup != null)
                    {
                        foreach (var area in _selectedRegionGroup.Areas.OrderBy(a => a.Name))
                        {
                            AreasInSelectedGroup.Add(area);
                        }
                    }
                }
            }
        }

        public AreaDefinition SelectedArea // New property
        {
            get => _selectedArea;
            set => SetProperty(ref _selectedArea, value);
        }

        // public string SelectedGroup // No longer needed
        // {
        //     get => _selectedGroup;
        //     set => SetProperty(ref _selectedGroup, value);
        // }

        public short SelectedMap
        {
            get => _selectedMap;
            set => SetProperty(ref _selectedMap, value);
        }

        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public int Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public RoomDefinition ToRoomDefinition()
        {
            var room = new RoomDefinition
            {
                Name = Name,
                // Logic to decide which group to use will be in the main view model.
                Group = !string.IsNullOrEmpty(NewGroupName) ? NewGroupName : (SelectedArea?.Group ?? SelectedRegionGroup?.Name ?? string.Empty),
                Map = SelectedMap,
                P = new System.Windows.Point(X, Y),
                Z = Z
            };

            // Add the drawn rectangle to the Rects list
            room.Rects.Add(new System.Windows.Rect(X1, Y1, Width, Height));

            return room;
        }

        // IDataErrorInfo implementation
        public string Error => null; // Not used for property-specific validation

        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            result = "Name cannot be empty.";
                        break;
                }
                return result;
            }
        }

        // Helper property to check if the ViewModel is valid
        public bool IsValid => string.IsNullOrWhiteSpace(this[nameof(Name)]);
    }
}