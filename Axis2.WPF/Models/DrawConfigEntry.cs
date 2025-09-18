using System.Collections.Generic;

namespace Axis2.WPF.Models
{
    public class DrawConfigEntry
    {
        public ushort Id { get; set; }
        public float Zoom { get; set; }
        public float Dezoom { get; set; }
        public float TimeZoom { get; set; }
        public float Rotation { get; set; }
        public float Alternance { get; set; }
        public int NumberOfColors { get; set; }
        public List<ushort> ColorIds { get; set; } = new List<ushort>();
        public string Comment { get; set; } = string.Empty;
    }
}