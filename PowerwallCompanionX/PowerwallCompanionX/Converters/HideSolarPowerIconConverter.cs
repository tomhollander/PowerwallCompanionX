using Android.Opengl;
using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.Converters
{
    class HideSolarPowerIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MainViewModel vm = ((Grid)parameter).BindingContext as MainViewModel;
            if (Math.Abs(vm.SolarValue - (double)value) < 50) // If the current bar is the only one, always show the icon
            {
                return true;
            }
            var graphScale = decimal.ToDouble(Settings.GraphScale);
            double scaledValue = (double)(value) / 30 / graphScale;
            if (scaledValue < 50)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
