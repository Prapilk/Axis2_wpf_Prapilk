using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Axis2.WPF.Converters
{
    public class ColorIdToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort colorId)
            {
                try
                {
                    var color = App.UoArtService.GetColorFromDrawConfig((ushort)value);
                    return new SolidColorBrush(color);
                }
                catch (Exception)
                {
                    return System.Windows.Media.Brushes.Transparent;
                }
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
