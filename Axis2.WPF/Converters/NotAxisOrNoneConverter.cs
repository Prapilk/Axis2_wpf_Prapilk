using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class NotAxisOrNoneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string profileName)
            {
                return !(profileName == "<Axis Profile>" || profileName == "<None>");
            }
            return true; // Default to enabled if not a string or null
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}