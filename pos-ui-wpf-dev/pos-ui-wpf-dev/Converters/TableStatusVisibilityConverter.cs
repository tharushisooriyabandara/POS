using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using POS_UI.Models;

namespace POS_UI.Converters
{
    public class TableStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TableStatus status || parameter is not string targetStatus)
            {
                return Visibility.Collapsed;
            }

            return status.ToString() == targetStatus ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 