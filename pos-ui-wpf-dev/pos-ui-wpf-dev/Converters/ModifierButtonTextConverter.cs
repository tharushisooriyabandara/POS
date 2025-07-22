using System;
using System.Globalization;
using System.Windows.Data;

namespace POS_UI.Converters
{
    public class ModifierButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedSize && !string.IsNullOrEmpty(selectedSize) && selectedSize != "Small")
            {
                return "Edit Modifiers";
            }
            return "Add Modifiers";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 