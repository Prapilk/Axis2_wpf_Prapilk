using System.ComponentModel;

namespace Axis2.WPF.Models
{
    public enum SObjectType
    {
        None,
        Item,
        Multi,
        Template,
        Npc,
        Area,
        Room,
        Spell,
        SpawnGroup
    }

    public class SObject : INotifyPropertyChanged
    {
        private string _id = "";
        private string _value = "";
        private string _description = "<unnamed>";
        private string _category = "<none>";
        private string _subSection = "<none>";
        private string _displayId = "";
        private string _color = "0";
        private string _dupeItem = "";
        private string _fileName = "";
        private string _explicitDefName = "";
        private string _scriptType = ""; // New property for script-defined TYPE
        private SObjectType _type = SObjectType.None;

        public string Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Value { get => _value; set { _value = value; OnPropertyChanged(nameof(Value)); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }
        public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }
        public string SubSection { get => _subSection; set { _subSection = value; OnPropertyChanged(nameof(SubSection)); } }
        public string DisplayId { get => _displayId; set { _displayId = value; OnPropertyChanged(nameof(DisplayId)); } }
        public string Color { get => _color; set { _color = value; OnPropertyChanged(nameof(Color)); } }
        public string DupeItem { get => _dupeItem; set { _dupeItem = value; OnPropertyChanged(nameof(DupeItem)); } }
        public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(nameof(FileName)); } }
        public string ExplicitDefName { get => _explicitDefName; set { _explicitDefName = value; OnPropertyChanged(nameof(ExplicitDefName)); } }
        public string ScriptType { get => _scriptType; set { _scriptType = value; OnPropertyChanged(nameof(ScriptType)); } } // New public property
        public SObjectType Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }

        public SpawnGroup? Group { get; set; } // New property for SpawnGroup data
        public MapRegion? Region { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public object Clone()
        {
            return new SObject
            {
                Id = this.Id,
                Value = this.Value,
                Description = this.Description,
                Category = this.Category,
                SubSection = this.SubSection,
                DisplayId = this.DisplayId,
                Color = this.Color,
                DupeItem = this.DupeItem,
                FileName = this.FileName,
                ExplicitDefName = this.ExplicitDefName,
                Type = this.Type
            };
        }
    }
}
