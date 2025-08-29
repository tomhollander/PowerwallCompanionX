using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    class KilowattConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scaledValue = (double)(value) / 1000;

            if (parameter != null && parameter.ToString() == "abs")
            {
                scaledValue = Math.Abs(scaledValue);
            }

            return scaledValue.ToString("f1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
