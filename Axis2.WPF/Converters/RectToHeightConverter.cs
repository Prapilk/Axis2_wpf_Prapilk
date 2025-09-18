using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class RectToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height && parameter is int zoomLevel)
            {
                double scaleFactor = zoomLevel >= 0 ? Math.Pow(2, zoomLevel) : 1.0 / Math.Pow(2, Math.Abs(zoomLevel));
                return height * scaleFactor;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
