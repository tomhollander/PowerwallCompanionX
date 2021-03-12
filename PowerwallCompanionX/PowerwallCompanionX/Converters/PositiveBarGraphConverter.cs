using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.Converters
{
    class PositiveBarGraphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            if (doubleValue < 0)
            {
                return 0D;
            }
            return doubleValue / 30 / (double)1; // Settings.GraphScale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
