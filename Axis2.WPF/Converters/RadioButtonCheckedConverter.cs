using System;
using System.Globalization;
using System.Windows.Data;

namespace Axis2.WPF.Converters
{
    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            string? checkValue = value.ToString();
            string? targetValue = parameter.ToString();
            return checkValue?.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;
            bool useValue = (bool)value;
            if (useValue)
            {
                return parameter;
            }
            return null;
        }
    }
}