using System;
using System.Globalization;
using System.Windows.Data;

namespace UsbSwitcher.Converter
{
    class BoolToServiceStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Not installed";

            if ((bool) value)
            {
                return "Running";
            }
            return "Not running";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
