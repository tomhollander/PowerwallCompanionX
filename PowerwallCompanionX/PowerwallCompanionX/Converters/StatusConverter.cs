using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.Converters
{
    class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ViewModels.MainViewModel.StatusEnum)value;
            switch (status)
            {
                case ViewModels.MainViewModel.StatusEnum.IdleGrid:
                    return new SolidColorBrush(Color.Lime);
                case ViewModels.MainViewModel.StatusEnum.ExportingToGrid:
                    return new SolidColorBrush(Color.Magenta);
                case ViewModels.MainViewModel.StatusEnum.ImportingFromGrid:
                    return new SolidColorBrush(Color.MediumBlue);
                case ViewModels.MainViewModel.StatusEnum.Error:
                default:
                    return new SolidColorBrush(Color.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
