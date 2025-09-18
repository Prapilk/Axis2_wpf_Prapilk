namespace Axis2.WPF.Models
{
    public class TravelLocation
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Map { get; set; }
    }
}