namespace Axis2.WPF.Models
{
    public class ScriptObject
    {
        public string Description { get; set; } = string.Empty;
        public string Subsection { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DupeItem { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty; // From CSObject's m_csValue, used for various things
        public string Filename { get; set; } = string.Empty; // From CTObject
    }
}