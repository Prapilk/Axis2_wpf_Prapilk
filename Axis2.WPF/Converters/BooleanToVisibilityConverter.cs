using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            if (parameter != null && parameter.ToString() == "Inverse")
            {
                booleanValue = !booleanValue;
            }

            if (booleanValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}