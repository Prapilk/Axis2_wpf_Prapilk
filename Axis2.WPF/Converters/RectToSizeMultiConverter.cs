using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class RectToSizeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double size && values[1] is int zoomLevel)
            {
                double scaleFactor = zoomLevel >= 0 ? Math.Pow(2, zoomLevel) : 1.0 / Math.Pow(2, Math.Abs(zoomLevel));
                return size * scaleFactor;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
