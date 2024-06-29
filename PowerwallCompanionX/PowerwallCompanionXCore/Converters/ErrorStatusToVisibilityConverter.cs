using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static PowerwallCompanionX.ViewModels.MainViewModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PowerwallCompanionX.Converters
{
    class ErrorStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (MainViewModel.StatusEnum)value;
            if (status == MainViewModel.StatusEnum.Error)
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
