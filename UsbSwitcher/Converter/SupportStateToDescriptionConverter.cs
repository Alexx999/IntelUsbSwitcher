using System;
using System.Globalization;
using System.Windows.Data;
using UsbSwitcher.Core;

namespace UsbSwitcher.Converter
{
    public class SupportStateToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "Unknown";
            }

            var state = (SupportState)value;

            switch (state)
            {

                case SupportState.Supported:
                {
                    return "Supported";
                }
                case SupportState.FailChipsetNotSupported:
                {
                    return "Chipset not supported";
                }
                case SupportState.FailDriverLoad:
                {
                    return "Driver load failed";
                }
                case SupportState.FailNoUsb30:
                {
                    return "xHCI Controller not found";
                }
                case SupportState.FailNotUsb30:
                {
                    return "xHCI Controller not Intel";
                }
                case SupportState.Unknown:
                {
                    return "Unknown";
                }
                case SupportState.WarnPrimaryEhciInactive:
                {
                    return "EHCI Controller #1 not found - check BIOS settings";
                }
                case SupportState.WarnSecondaryEhciInactive:
                {
                    return "EHCI Controller #2 not found - check BIOS settings";
                }
                default:
                {
                    return "Unknown";
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
