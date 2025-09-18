using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class SecondsToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float seconds && seconds > 0)
            {
                return new Duration(TimeSpan.FromSeconds(seconds));
            }
            return Duration.Automatic;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
