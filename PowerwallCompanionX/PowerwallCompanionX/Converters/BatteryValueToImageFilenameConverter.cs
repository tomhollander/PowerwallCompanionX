using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.Converters
{
    internal class BatteryValueToImageFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double batteryValue = (double)value;
            if (batteryValue < -20)
            {
                return "icon_battery_import.png";
            }
            else if (batteryValue > 20)
            {
                return "icon_battery_export.png";
            }
            else
            {
                return null;
            }
          
        }
    

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
