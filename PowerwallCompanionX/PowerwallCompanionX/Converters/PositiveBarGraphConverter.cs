﻿using System;
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
            return doubleValue / 30 / GetGraphScale();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double GetGraphScale()
        {
            if (Application.Current.Properties.ContainsKey(AppProperties.GraphScale))
            {
                return decimal.ToDouble((decimal)Application.Current.Properties[AppProperties.GraphScale]);
            }
            return 1;
        }
    }
}