using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using static Android.App.Assist.AssistStructure;

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
