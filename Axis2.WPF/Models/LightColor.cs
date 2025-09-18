using System.Windows.Media;

namespace Axis2.WPF.Models
{
    public class LightColor
    {
        public ushort Id { get; set; }
        public ushort DrawConfigId { get; set; }
        public System.Windows.Media.Color ColorValue { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}