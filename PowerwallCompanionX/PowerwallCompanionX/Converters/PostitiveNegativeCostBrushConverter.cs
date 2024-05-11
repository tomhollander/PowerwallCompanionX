using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace PowerwallCompanionX.Converters
{
    public class PostitiveNegativeCostBrushConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cost = System.Convert.ToDecimal(value);
            if (cost > 0M)
            {
                return Color.LightGray;
            }
            else
            {
                return Color.LightGreen;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
