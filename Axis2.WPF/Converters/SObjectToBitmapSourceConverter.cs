using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Axis2.WPF.Models;
using Axis2.WPF.Services;
using Axis2.WPF.Extensions;

namespace Axis2.WPF.Converters
{
    public class SObjectToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SObject sObject)
            {
                return App.UoArtService.GetItemArt((int)sObject.Id.AllToUInt(), 0);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
