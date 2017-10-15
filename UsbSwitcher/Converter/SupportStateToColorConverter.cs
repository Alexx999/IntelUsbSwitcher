using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UsbSwitcher.Core;

namespace UsbSwitcher.Converter
{
    public class SupportStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            var state = (SupportState) value;

            switch (state)
            {
                case SupportState.Supported:
                {
                    return new SolidColorBrush(Colors.Green);
                }
                case SupportState.FailChipsetNotSupported:
                case SupportState.FailDriverLoad:
                case SupportState.FailNoUsb30:
                case SupportState.FailNotUsb30:
                {
                    return new SolidColorBrush(Colors.Red);
                }
                case SupportState.Unknown:
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                case SupportState.WarnPrimaryEhciInactive:
                case SupportState.WarnSecondaryEhciInactive:
                {
                    return new SolidColorBrush(Colors.Yellow);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
