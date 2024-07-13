using PowerwallCompanionX.ViewModels;
using System.Text;

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

        private async void SettingsLabelTapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                // Copy debug info to clipboard
                var sb = new StringBuilder();
                sb.AppendLine($"SiteId: {Settings.SiteId}");
                sb.AppendLine($"InstallationTimeZone: {Settings.InstallationTimeZone}");
                sb.AppendLine($"SystemTimeZone: {TimeZoneInfo.Local.Id}");
                sb.AppendLine($"AccessToken: {Settings.AccessToken}");
                await Clipboard.Default.SetTextAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
            }
        }
    }
}