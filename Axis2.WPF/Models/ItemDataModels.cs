using System.Collections.ObjectModel;

namespace Axis2.WPF.Models
{
    public class CSObject
    {
        public string Description { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Type { get; set; } // Corresponds to m_bType in C++
        public string Filename { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Subsection { get; set; } = string.Empty;
        public string DupeItem { get; set; } = string.Empty;
        public bool IsCustom { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CSubsection
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<CSObject> ItemList { get; set; }

        public CSubsection()
        {
            ItemList = new ObservableCollection<CSObject>();
        }
    }

    public class CCategory
    {
        public string Name { get; set; } = string.Empty;
        public List<CSObject> ItemList { get; set; } = new List<CSObject>();
        public ObservableCollection<CSubsection> SubsectionList { get; set; }

        public CCategory()
        {
            SubsectionList = new ObservableCollection<CSubsection>();
        }
    }
}