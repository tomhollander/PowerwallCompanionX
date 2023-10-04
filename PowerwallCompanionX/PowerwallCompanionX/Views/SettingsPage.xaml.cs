using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerwallCompanionX.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel viewModel;
        public SettingsPage()
        {
            InitializeComponent();
            viewModel = new SettingsViewModel();
            this.BindingContext = viewModel;
            weatherUnitsSegmentedControl.SelectedIndex = viewModel.WeatherUnits == "C" ? 0 : 1;
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
            {
                cyclePages.IsEnabled = false;
            }
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            teslaCarSettings.IsVisible = (viewModel.SelectedExtras.Key == "Tesla");
            weatherSettings.IsVisible = (viewModel.SelectedExtras.Key == "Weather");
            amberSettings.IsVisible = (viewModel.SelectedExtras.Key == "Amber");
            newsSettings.IsVisible = (viewModel.SelectedExtras.Key == "News");
        }

        private void weatherUnitsSegmentedControl_SelectionChanged(object sender, Syncfusion.XForms.Buttons.SelectionChangedEventArgs e)
        {
            viewModel.WeatherUnits = weatherUnitsSegmentedControl.SelectedIndex == 0 ? "C" : "F";
        }
    }
}