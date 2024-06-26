﻿using PowerwallCompanionX.ViewModels;
using System.Globalization;

namespace PowerwallCompanionX.Converters
{
    class StatusToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (MainViewModel.StatusEnum)value;
            if (status == MainViewModel.StatusEnum.Online)
            {
                return Colors.DimGray;
            }
            else if (status == MainViewModel.StatusEnum.GridOutage)
            {
                return Colors.SaddleBrown;
            }
            else if (status == MainViewModel.StatusEnum.Error)
            {
                return Color.FromRgb(50, 50, 50);
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
