using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using static PowerwallCompanionX.ViewModels.MainViewModel;

namespace PowerwallCompanionX.Converters
{
    class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (MainViewModel.StatusEnum)value;
            if (status == MainViewModel.StatusEnum.Online)
            {
                return Color.LimeGreen;
            }
            else if (status == MainViewModel.StatusEnum.GridOutage)
            {
                return Color.Orange;
            }
            else if (status == MainViewModel.StatusEnum.Error)
            {
                return Color.DarkGray;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
