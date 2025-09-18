using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Axis2.WPF.Converters
{
    public class IsSelectedPathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return false;
            }

            object currentItem = values[0]; // This will be Category or SubCategory
            object selectedPathItem = values[1]; // This will be SelectedCategoryInTree or SelectedSubCategoryInTree

            return currentItem == selectedPathItem;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}