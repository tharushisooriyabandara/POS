using System;
using System.Globalization;
using System.Windows.Data;
using POS_UI.ViewModels;

namespace POS_UI.Converters
{
    public class SortOptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProductSortOption option)
            {
                switch (option)
                {
                    case ProductSortOption.AZ: return "A - Z";
                    case ProductSortOption.ZA: return "Z - A";
                    case ProductSortOption.PriceLowHigh: return "low - high";
                    case ProductSortOption.PriceHighLow: return "high - low";
                    default: return string.Empty;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 