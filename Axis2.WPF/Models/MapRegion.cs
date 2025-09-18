using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Axis2.WPF.ViewModels;
using System.Windows.Data;

namespace Axis2.WPF.Models
{
    public abstract class MapRegion : ViewModelBase
        {
            private string _name = string.Empty; // Backing field for Name
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }
        public string Group { get; set; } = string.Empty;
        public List<Rect> Rects { get; set; } = new List<Rect>();
        public System.Windows.Point P { get; set; }
        public int Z { get; set; } // Added Z coordinate
        public int Map { get; set; } // Added Map number

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }

    public class AreaDefinition : MapRegion, System.IEquatable<AreaDefinition>
    {
        public List<RoomDefinition> Rooms { get; set; } = new List<RoomDefinition>();

        public bool Equals(AreaDefinition other)
        {
            if (other is null)
                return false;

            return this.Name == other.Name && this.Group == other.Group && this.Map == other.Map;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AreaDefinition);
        }

        public override int GetHashCode()
        {
            return (Name, Group, Map).GetHashCode();
        }
    }

    public class RoomDefinition : MapRegion, System.IEquatable<RoomDefinition> 
    { 
        public string DefName { get; set; } = string.Empty;

        public bool Equals(RoomDefinition other)
        {
            if (other is null)
                return false;

            // Using Name, Group, and Map for equality check.
            // This assumes that a room is uniquely identified by its name within a specific group and map.
            return this.Name == other.Name && this.Group == other.Group && this.Map == other.Map;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomDefinition);
        }

        public override int GetHashCode()
        {
            // Using Name, Group, and Map for hash code generation.
            return (Name, Group, Map).GetHashCode();
        }
    }

    public class RegionGroup : ViewModelBase
    {
        public string Name { get; set; } = string.Empty;
        public List<AreaDefinition> Areas { get; set; } = new List<AreaDefinition>();
        public List<RoomDefinition> Rooms { get; set; } = new List<RoomDefinition>();

        public CompositeCollection Children
        {
            get
            {
                return new CompositeCollection
                {
                    new CollectionContainer { Collection = Areas },
                    new CollectionContainer { Collection = Rooms }
                };
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }
}