using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    class KilowattHourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scaledValue = (double)(value) / 1000;
            return scaledValue.ToString("f0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
