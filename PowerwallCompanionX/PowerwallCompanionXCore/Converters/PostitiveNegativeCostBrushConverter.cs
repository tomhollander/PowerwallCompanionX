using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    public class PostitiveNegativeCostBrushConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cost = System.Convert.ToDecimal(value);
            if (cost > 0M)
            {
                return Colors.LightGray;
            }
            else
            {
                return Colors.LightGreen;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
