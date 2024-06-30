using PowerwallCompanionX.ViewModels;
using System.Globalization;

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
