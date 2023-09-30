using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using static Android.App.Assist.AssistStructure;

namespace PowerwallCompanionX.Converters
{
    internal class GridValueToImageFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double gridValue = (double)value;
            if (gridValue < -100)
            {
                return "icon_grid_export_yellow.png";
            }
            else if (gridValue > 100)
            {
                return "icon_grid_import.png";
            }
            else
            {
                return null;
            }

        }
    

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
