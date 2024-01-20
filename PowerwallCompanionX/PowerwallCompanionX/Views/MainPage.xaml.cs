using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using PanCardView;
using PowerwallCompanionX.Extras;
using PowerwallCompanionX.Media;
using PowerwallCompanionX.ViewModels;
using Syncfusion.SfChart.XForms;
using Syncfusion.XForms.TextInputLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PowerwallCompanionX.Views
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel viewModel;
        private readonly TimeSpan liveStatusRefreshInterval = new TimeSpan(0, 0, 30);
        private readonly TimeSpan energyHistoryRefreshInterval = new TimeSpan(0, 5, 0);
        private readonly TimeSpan powerHistoryRefreshInterval = new TimeSpan(0, 5, 0);

        private DateTime lastManualSwipe;
        private readonly TimeSpan swipeIdlePeriod = new TimeSpan(0, 1, 0);

        private double maxPercentSinceSound = 0D;
        private double minPercentSinceSound = 100D;

        private bool keepRefreshing = true;
        private string lastOrientation;

        private Thickness timeDefaultMargin;

        private IExtrasProvider extrasProvider;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            SetPhoneOrTabletLayout();
            this.SizeChanged += MainPage_SizeChanged;
            this.BindingContext = viewModel;

            switch (Settings.SelectedExtras)
            {
                case "Powerwall":
                    extrasProvider = new PowerwallExtrasProvider(viewModel);
                    break;
                case "Weather":
                    extrasProvider = new WeatherExtrasProvider(Settings.WeatherCity, Settings.WeatherUnits);
                    break;
                case "Amber":
                    extrasProvider = new AmberExtrasProvider(Settings.AmberApiKey);
                    break;
                case "Tesla":
                    extrasProvider = new TeslaExtrasProvider();
                    break;
                case "News":
                    extrasProvider = new NewsExtrasProvider(Settings.NewsFeedUrl);
                    break;
                default:
                    extrasProvider = new OnboardingExtrasProvider();
                    break;
            }

            Task.Run(() => RefreshData());
        }

        private void SetPhoneOrTabletLayout()
        {
            timeDefaultMargin = timeTextBlock.Margin;

            if (ShowTwoPages())
            {
                rootGrid.Children.Remove(mainGrid);
                rootGrid.Children.Remove(dailyEnergyGrid);
                carousel.ItemsSource = new View[] { mainGrid, dailyEnergyGrid };
                carousel.IsVisible = true;
            }
            else
            {
                // Move time to the right to leave room for persistent settings button
                timeTextBlock.Margin = new Thickness(timeDefaultMargin.Left + 20, timeDefaultMargin.Top, timeDefaultMargin.Right, timeDefaultMargin.Bottom);

                rootGrid.Children.Remove(mainGrid);
                rootGrid.Children.Remove(dailyEnergyGrid);
                tabletGrid.Children.Add(mainGrid);
                tabletGrid.Children.Add(dailyEnergyGrid);
                settingsButton.Opacity = 1;
                carousel.IsVisible = false;
            }
            MainPage_SizeChanged(null, null);
        }

        private bool ShowTwoPages()
        {
            var formfactor = DeviceInfo.Idiom;
            if (formfactor == DeviceIdiom.Phone)
            {
                return true;
            }
            else
            {
                return Settings.TwoPagesOnTablet; // Configurable
            }
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {

            string visualState = Width > Height ? "Landscape" : "Portrait";

            if (ShowTwoPages())
            {
                if (visualState == "Portrait")
                {
                    // Give extra space for the clock
                    timeTextBlock.Margin = new Thickness(timeDefaultMargin.Left, timeDefaultMargin.Top + 15, timeDefaultMargin.Right, timeDefaultMargin.Bottom);

                    SetPortraitMode(mainGrid, dailyEnergyGrid);
                }
                else if (visualState == "Landscape")
                {
                    // Restore default margins for clock 
                    timeTextBlock.Margin = timeDefaultMargin;

                    SetLandscapeMode(mainGrid, dailyEnergyGrid);
                }
            }
            else
            {
                if (visualState == "Portrait")
                {
                    // Tablet portrait has two landscapes stacked
                    tabletGrid.ColumnDefinitions.Clear();
                    tabletGrid.RowDefinitions.Clear();
                    tabletGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
                    tabletGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) });
                    SetLandscapeMode(mainGrid, dailyEnergyGrid);
                    Grid.SetRow(mainGrid, 0);
                    Grid.SetRow(dailyEnergyGrid, 1);
                    Grid.SetColumn(mainGrid, 0);
                    Grid.SetColumn(dailyEnergyGrid, 0);
                }
                else
                {
                    // Tablet landscape has two portraits side by side
                    tabletGrid.RowDefinitions.Clear();
                    tabletGrid.ColumnDefinitions.Clear();
                    tabletGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    tabletGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    SetPortraitMode(mainGrid, dailyEnergyGrid);
                    Grid.SetColumn(mainGrid, 0);
                    Grid.SetColumn(dailyEnergyGrid, 1);
                    Grid.SetRow(mainGrid, 0);
                    Grid.SetRow(dailyEnergyGrid, 0);
                }
            }

            if (chart.Height > 200)
            {
                chart.HeightRequest = 200;
            }


        }

        private void SetLandscapeMode(Grid page1Grid, Grid page2Grid)
        {
            if (lastOrientation == "Landscape") return;

            // Page 1 : Move graph from row to column
            page1Grid.RowDefinitions[0].Height = GridLength.Auto;
            page1Grid.RowDefinitions[1].Height = new GridLength(0);
            page1Grid.ColumnDefinitions[0].Width = new GridLength(330);
            page1Grid.ColumnDefinitions[1].Width = GridLength.Auto;
            Grid.SetRow(page1Grid.Children[1], 0);
            Grid.SetColumn(page1Grid.Children[1], 1);

            // Page 2 : Transpose grid
            page2Grid.ColumnDefinitions.Clear();
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            for (int i = 0; i < 4; i++)
            {
                page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            }
            page2Grid.RowDefinitions.Clear();
            for (int i = 0; i < 4; i++)
            {
                page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            var chart = page2Grid.Children.Last();
            if (Grid.GetRow(chart) == 6)
            {
                // Previously in portrait mode, so transpose
                foreach (var item in page2Grid.Children)
                {
                    if (item != chart)
                    {
                        int currentRow = Grid.GetRow(item);
                        int currentColumn = Grid.GetColumn(item);
                        Grid.SetColumn(item, currentRow);
                        Grid.SetRow(item, currentColumn);
                    }
                    else
                    {
                        Grid.SetRow(chart, 3);
                        Grid.SetColumnSpan(chart, 5);
                    }
                }
            }
            lastOrientation = "Landscape";
        }

        private void SetPortraitMode(Grid page1Grid, Grid page2Grid)
        {
            if (lastOrientation == "Portrait") return;

            // Page 1 : Move graph from column to row
            page1Grid.RowDefinitions[0].Height = new GridLength(3, GridUnitType.Star);
            page1Grid.RowDefinitions[1].Height = new GridLength(4, GridUnitType.Star);
            page1Grid.ColumnDefinitions[0].Width = GridLength.Star;
            page1Grid.ColumnDefinitions[1].Width = new GridLength(0);
            Grid.SetRow(page1Grid.Children[1], 1);
            Grid.SetColumn(page1Grid.Children[1], 0);

            // Page 2 : Transpose grid
            page2Grid.RowDefinitions.Clear();
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            for (int i = 0; i < 4; i++)
            {
                page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
            }
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            page2Grid.ColumnDefinitions.Clear();
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            var chart = page2Grid.Children.Last();
            if (Grid.GetRow(chart) == 3)
            {
                // Previously in landscape mode, so transpose
                foreach (var item in page2Grid.Children)
                {
                    if (item != chart)
                    {
                        int currentRow = Grid.GetRow(item);
                        int currentColumn = Grid.GetColumn(item);
                        Grid.SetColumn(item, currentRow);
                        Grid.SetRow(item, currentColumn);
                    }
                    else
                    {
                        Grid.SetRow(chart, 6);
                        Grid.SetColumnSpan(chart, 3);
                    }
                }
            }
            lastOrientation = "Portrait";
        }

        private async Task RefreshData()
        {
            Analytics.TrackEvent("Data refreshed");
            extrasTextBlock.IsVisible = (extrasProvider != null);
            await RefreshDataFromTeslaOwnerApi();
            viewModel.ExtrasContent = await extrasProvider?.RefreshStatus();

            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
                {
                Task.Run(async () =>
                {
                    await RefreshDataFromTeslaOwnerApi();
                    viewModel.ExtrasContent = await extrasProvider?.RefreshStatus();

                });
                if (ShowTwoPages() &&
                    Settings.CyclePages && DateTime.Now - lastManualSwipe > swipeIdlePeriod)
                {
                    carousel.SelectedIndex = (carousel.SelectedIndex + 1) % 2;
                }

                return keepRefreshing; // True = Repeat again, False = Stop the timer

            });

        }

        protected override void OnDisappearing()
        {
            keepRefreshing = false;
        }

        private async Task RefreshDataFromTeslaOwnerApi()
        {
            await GetCurrentPowerData();
            await GetEnergyHistoryData();
            await GetPowerHistoryData();
        }

        private async Task GetCurrentPowerData()
        { 
            try
            {
                if (DateTime.Now - viewModel.LiveStatusLastRefreshed < liveStatusRefreshInterval)
                {
                    return;
                }
#if FAKE
                viewModel.BatteryPercent = 72;
                viewModel.HomeValue = 1900D;
                viewModel.SolarValue = 1900D;
                viewModel.BatteryValue = -1000D;
                viewModel.GridValue = 100D;
                viewModel.GridActive = true;
#else
                var siteId = Settings.SiteId;

                var powerInfo = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{siteId}/live_status", "LiveStatus");

                viewModel.BatteryPercent = GetJsonDoubleValue(powerInfo["response"]["energy_left"]) / GetJsonDoubleValue(powerInfo["response"]["total_pack_energy"]) * 100D;
                viewModel.HomeValue = GetJsonDoubleValue(powerInfo["response"]["load_power"]);
                viewModel.SolarValue = GetJsonDoubleValue(powerInfo["response"]["solar_power"]);
                viewModel.BatteryValue = GetJsonDoubleValue(powerInfo["response"]["battery_power"]);
                viewModel.GridValue = GetJsonDoubleValue(powerInfo["response"]["grid_power"]);
                viewModel.GridActive = powerInfo["response"]["grid_status"].Value<string>() != "Inactive";
                viewModel.TotalPackEnergy = GetJsonDoubleValue(powerInfo["response"]["total_pack_energy"]);  
                viewModel.Status = viewModel.GridActive ? MainViewModel.StatusEnum.Online : MainViewModel.StatusEnum.GridOutage;
#endif
                viewModel.LiveStatusLastRefreshed = DateTime.Now;
                viewModel.NotifyPowerProperties();


                PlaySoundsOnBatteryStatus(viewModel.BatteryPercent);
            }
            catch (UnauthorizedAccessException ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (!(Application.Current.MainPage is LoginPage))
                    {
                        Application.Current.MainPage = new LoginPage();
                        viewModel.LastExceptionMessage = ex.Message;
                        viewModel.LastExceptionDate = DateTime.Now;
                        viewModel.NotifyPowerProperties();
                        viewModel.Status = MainViewModel.StatusEnum.Error;
                    }
                });
                
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyPowerProperties();
                viewModel.Status = MainViewModel.StatusEnum.Error;
            }
        }

        private void PlaySoundsOnBatteryStatus(double newPercent)
        {
            if (Settings.PlaySounds)
            {
                minPercentSinceSound = Math.Min(minPercentSinceSound, newPercent);
                maxPercentSinceSound = Math.Max(maxPercentSinceSound, newPercent);
                if (newPercent >= 99.6D && minPercentSinceSound < 80D)
                {
                    var player = new SoundPlayer();
                    player.PlaySound(SoundPlayer.BatteryFull);
                    minPercentSinceSound = 100D;
                }
                else if (newPercent <= 0.5D && maxPercentSinceSound > 20D)
                {
                    var player = new SoundPlayer();
                    player.PlaySound(SoundPlayer.BatteryEmpty);
                    maxPercentSinceSound = 0D;
                }
            }
        }
        private async Task GetEnergyHistoryData()
        {
            try
            {
                if (DateTime.Now - viewModel.EnergyHistoryLastRefreshed < energyHistoryRefreshInterval)
                {
                    return;
                }

                string period = "day";
                var json = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{Settings.SiteId}/history?kind=energy&period={period}", "EnergyHistory");

                var yesterday = json["response"]["time_series"][0];
                viewModel.HomeEnergyYesterday = GetJsonDoubleValue(yesterday["consumer_energy_imported_from_grid"]) + GetJsonDoubleValue(yesterday["consumer_energy_imported_from_solar"]) + GetJsonDoubleValue(yesterday["consumer_energy_imported_from_battery"])+ GetJsonDoubleValue(yesterday["consumer_energy_imported_from_generator"]);
                viewModel.SolarEnergyYesterday = GetJsonDoubleValue(yesterday["solar_energy_exported"]);
                viewModel.GridEnergyImportedYesterday = GetJsonDoubleValue(yesterday["grid_energy_imported"]);
                viewModel.GridEnergyExportedYesterday = GetJsonDoubleValue(yesterday["grid_energy_exported_from_solar"]) + GetJsonDoubleValue(yesterday["grid_energy_exported_from_battery"]);
                viewModel.BatteryEnergyImportedYesterday = GetJsonDoubleValue(yesterday["battery_energy_imported_from_grid"]) + GetJsonDoubleValue(yesterday["battery_energy_imported_from_solar"]);
                viewModel.BatteryEnergyExportedYesterday = GetJsonDoubleValue(yesterday["battery_energy_exported"]);

                var today = json["response"]["time_series"][1];
                viewModel.HomeEnergyToday = GetJsonDoubleValue(today["consumer_energy_imported_from_grid"]) + GetJsonDoubleValue(today["consumer_energy_imported_from_solar"]) + GetJsonDoubleValue(today["consumer_energy_imported_from_battery"]) + GetJsonDoubleValue(today["consumer_energy_imported_from_generator"]);
                viewModel.SolarEnergyToday = GetJsonDoubleValue(today["solar_energy_exported"]);
                viewModel.GridEnergyImportedToday = GetJsonDoubleValue(today["grid_energy_imported"]);
                viewModel.GridEnergyExportedToday = GetJsonDoubleValue(today["grid_energy_exported_from_solar"]) + GetJsonDoubleValue(today["grid_energy_exported_from_battery"]);
                viewModel.BatteryEnergyImportedToday = GetJsonDoubleValue(today["battery_energy_imported_from_grid"]) + GetJsonDoubleValue(today["battery_energy_imported_from_solar"]);
                viewModel.BatteryEnergyExportedToday = GetJsonDoubleValue(today["battery_energy_exported"]);

                viewModel.EnergyHistoryLastRefreshed = DateTime.Now;
                viewModel.NotifyDailyEnergyProperties();

                // This should be possible with data binding, but for some reason it is failing on phones in portrait mode
                Device.BeginInvokeOnMainThread(() =>
                {
                    bothGridSettingsToday.IsVisible = viewModel.ShowBothGridSettingsToday;
                    bothGridSettingsYesterday.IsVisible = viewModel.ShowBothGridSettingsYesterday;
                    singleGridSettingsToday.IsVisible = !viewModel.ShowBothGridSettingsToday; 
                    singleGridSettingsYesterday.IsVisible = !viewModel.ShowBothGridSettingsYesterday;
                });


            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyDailyEnergyProperties();
            }
        }

        public async Task GetPowerHistoryData()
        {
            try
            {
                if (DateTime.Now - viewModel.PowerHistoryLastRefreshed < powerHistoryRefreshInterval || !Settings.ShowGraph)
                {
                    return;
                }

                var json = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{Settings.SiteId}/history?kind=power", "PowerHistory");
                var homeGraphData = new List<ChartDataPoint>();
                var solarGraphData = new List<ChartDataPoint>();
                var batteryGraphData = new List<ChartDataPoint>();
                var gridGraphData = new List<ChartDataPoint>();

                foreach (var datapoint in (JArray)json["response"]["time_series"])
                {
                    var timestamp = datapoint["timestamp"].Value<DateTime>();
                    var solarPower = datapoint["solar_power"].Value<double>() / 1000;
                    var batteryPower = datapoint["battery_power"].Value<double>() / 1000;
                    var gridPower = datapoint["grid_power"].Value<double>() / 1000;
                    var homePower = solarPower + batteryPower + gridPower;
                    homeGraphData.Add(new ChartDataPoint(timestamp, homePower));
                    solarGraphData.Add(new ChartDataPoint(timestamp, solarPower));
                    gridGraphData.Add(new ChartDataPoint(timestamp, gridPower));
                    batteryGraphData.Add(new ChartDataPoint(timestamp, batteryPower));
                }

                viewModel.HomeGraphData = homeGraphData;
                viewModel.SolarGraphData = solarGraphData;
                viewModel.GridGraphData = gridGraphData;
                viewModel.BatteryGraphData = batteryGraphData;
                viewModel.PowerHistoryLastRefreshed = DateTime.Now;
                viewModel.NotifyGraphProperties();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.LastExceptionMessage = ex.Message;
            }
        }


        private static double GetJsonDoubleValue(JToken jtoken)
        {
            if (jtoken == null)
            {
                return 0;
            }
            try
            {
                return jtoken.Value<double>();
            }
            catch
            {
                return 0;
            }
        }

        private void CarouselView_ItemAppeared(PanCardView.CardsView view, PanCardView.EventArgs.ItemAppearedEventArgs args)
        {
            if (args.Type == PanCardView.Enums.InteractionType.User)
            {
                lastManualSwipe = DateTime.Now;
                ShowSettingsButtonThenFade();
            }
            
        }

        private async void status_Tapped(object sender, EventArgs e)
        {
            if (viewModel.Status == MainViewModel.StatusEnum.Error)
            {
                if (viewModel.LastExceptionMessage != null)
                {
                    var message = $"Last error occurred at {viewModel.LastExceptionDate.ToString("g")}:\r\n{viewModel.LastExceptionMessage}";
                    await Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
                }
            }
            await ShowSettingsButtonThenFade();
        }



        private async void grid_Tapped(object sender, EventArgs e)
        {
            await ShowSettingsButtonThenFade();
        }

        private async Task ShowSettingsButtonThenFade()
        {
            if (ShowTwoPages())
            {
                await settingsButton.FadeTo(1, 500);
                await Task.Delay(5000);
                await settingsButton.FadeTo(0, 500);
            }
        }
    }

}
