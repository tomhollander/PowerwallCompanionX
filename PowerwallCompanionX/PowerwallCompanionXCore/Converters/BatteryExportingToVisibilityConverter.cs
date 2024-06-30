using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    internal class BatteryExportingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double batteryValue = (double)value;
            if (batteryValue > 20)
            {
                return true;
            }
            return false;
        }
    

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
