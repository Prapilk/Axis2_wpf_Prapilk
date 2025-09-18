using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class RectToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Rect rect)
            {
                return new System.Windows.Size(rect.Width, rect.Height);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}