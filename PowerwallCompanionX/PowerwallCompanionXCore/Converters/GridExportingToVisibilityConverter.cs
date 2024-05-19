using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    internal class GridExportingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double gridValue = (double)value;

            if (gridValue < -100)
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
