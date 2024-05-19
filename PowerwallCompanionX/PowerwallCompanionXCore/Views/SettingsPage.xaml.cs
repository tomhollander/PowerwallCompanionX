using PowerwallCompanionX.ViewModels;

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
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            weatherSettings.IsVisible = (viewModel.SelectedExtras.Key == "Weather");
            amberSettings.IsVisible = (viewModel.SelectedExtras.Key == "Amber");
            newsSettings.IsVisible = (viewModel.SelectedExtras.Key == "News");
            powerwallSettings.IsVisible = (viewModel.SelectedExtras.Key == "Powerwall");
        }

        private void weatherUnitsSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
        {
            viewModel.WeatherUnits = weatherUnitsSegmentedControl.SelectedIndex == 0 ? "C" : "F";
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://ko-fi.com/tomhollander");
        }
    }
}