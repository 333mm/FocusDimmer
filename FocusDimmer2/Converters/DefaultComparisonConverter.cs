using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FocusDimmer.Converters
{
    public class DefaultComparisonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is string currentId && values[1] is string defaultId)
            {
                if (string.IsNullOrEmpty(currentId)) return Visibility.Collapsed;
                return currentId == defaultId ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
