using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class RectToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is int zoomLevel)
            {
                double scaleFactor = zoomLevel >= 0 ? Math.Pow(2, zoomLevel) : 1.0 / Math.Pow(2, Math.Abs(zoomLevel));
                return width * scaleFactor;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
