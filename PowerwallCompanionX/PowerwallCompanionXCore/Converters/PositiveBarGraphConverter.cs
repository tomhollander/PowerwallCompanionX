using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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
            return doubleValue / 30 / GetGraphScale();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double GetGraphScale()
        {
               return Settings.GraphScale;   
        }
    }
}
