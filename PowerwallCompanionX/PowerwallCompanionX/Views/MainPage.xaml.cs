using Newtonsoft.Json.Linq;
using PowerwallCompanionX.Media;
using PowerwallCompanionX.ViewModels;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            this.SizeChanged += MainPage_SizeChanged;
            this.BindingContext = viewModel;

            Task.Run(() => RefreshData());
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {

            string visualState = Width > Height ? "Landscape" : "Portrait";
            var page1Grid = (Grid)((Array)carousel.ItemsSource).GetValue(0);
            var page2Grid = (Grid)((Array)carousel.ItemsSource).GetValue(1);

            if (visualState == "Portrait")
            {
                // Page 1 : Move graph from column to row
                page1Grid.RowDefinitions[0].Height = new GridLength(330);
                page1Grid.RowDefinitions[1].Height = GridLength.Auto;
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
                
            }
            else
            {
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
                        } else
                        {
                            Grid.SetRow(chart, 3);
                            Grid.SetColumnSpan(chart, 5);
                        }
                    }
                }
            }    
           
        }

        private async Task RefreshData()
        { 
            await RefreshDataFromTeslaOwnerApi();

            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                RefreshDataFromTeslaOwnerApi();
                if (Settings.CyclePages && DateTime.Now - lastManualSwipe > swipeIdlePeriod)
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

                var powerInfo = await ApiHelper.CallGetApiWithTokenRefresh($"{ApiHelper.BaseUrl}/api/1/energy_sites/{siteId}/live_status", "LiveStatus");

                viewModel.BatteryPercent = GetJsonDoubleValue(powerInfo["response"]["energy_left"]) / GetJsonDoubleValue(powerInfo["response"]["total_pack_energy"]) * 100D;
                viewModel.HomeValue = GetJsonDoubleValue(powerInfo["response"]["load_power"]);
                viewModel.SolarValue = GetJsonDoubleValue(powerInfo["response"]["solar_power"]);
                viewModel.BatteryValue = GetJsonDoubleValue(powerInfo["response"]["battery_power"]);
                viewModel.GridValue = GetJsonDoubleValue(powerInfo["response"]["grid_power"]);
                viewModel.GridActive = powerInfo["response"]["grid_status"].Value<string>() != "Inactive";
#endif
                viewModel.LiveStatusLastRefreshed = DateTime.Now;
                
                if (viewModel.GridValue < -100)
                {
                    viewModel.Status = MainViewModel.StatusEnum.ExportingToGrid;
                }
                else if (viewModel.GridValue > 100)
                {
                    viewModel.Status = MainViewModel.StatusEnum.ImportingFromGrid;
                } 
                else
                {
                    viewModel.Status = MainViewModel.StatusEnum.IdleGrid;
                }
                viewModel.NotifyProperties();


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
                        viewModel.NotifyProperties();
                        viewModel.Status = MainViewModel.StatusEnum.Error;
                    }
                });
                
            }
            catch (Exception ex)
            {
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyProperties();
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
                var json = await ApiHelper.CallGetApiWithTokenRefresh($"{ApiHelper.BaseUrl}/api/1/energy_sites/{Settings.SiteId}/history?kind=energy&period={period}", "EnergyHistory");

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
                viewModel.NotifyProperties();
   

            }
            catch (Exception ex)
            {
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyProperties();
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

                var json = await ApiHelper.CallGetApiWithTokenRefresh($"{ApiHelper.BaseUrl}/api/1/energy_sites/{Settings.SiteId}/history?kind=power", "PowerHistory");
                viewModel.HomeGraphData = new List<ChartDataPoint>();
                viewModel.SolarGraphData = new List<ChartDataPoint>();
                viewModel.BatteryGraphData = new List<ChartDataPoint>();
                viewModel.GridGraphData = new List<ChartDataPoint>();

                foreach (var datapoint in (JArray)json["response"]["time_series"])
                {
                    var timestamp = datapoint["timestamp"].Value<DateTime>();
                    var solarPower = datapoint["solar_power"].Value<double>();
                    var batteryPower = datapoint["battery_power"].Value<double>();
                    var gridPower = datapoint["grid_power"].Value<double>();
                    var homePower = solarPower + batteryPower + gridPower;
                    viewModel.HomeGraphData.Add(new ChartDataPoint(timestamp, homePower));
                    viewModel.SolarGraphData.Add(new ChartDataPoint(timestamp, solarPower));
                    viewModel.GridGraphData.Add(new ChartDataPoint(timestamp, gridPower));
                    viewModel.BatteryGraphData.Add(new ChartDataPoint(timestamp, batteryPower));
                }

                viewModel.PowerHistoryLastRefreshed = DateTime.Now;
                viewModel.NotifyGraphProperties();
            }
            catch (Exception ex)
            {
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
            }

            if (args.Index == 0)
            {
                unitsLabel.Text = "kW";
            }
            else if (args.Index == 1)
            {
                unitsLabel.Text = "kWh";
            }
        }

        private async void statusEllipse_Tapped(object sender, EventArgs e)
        {

            if (viewModel.Status == MainViewModel.StatusEnum.Error)
            {
                if (viewModel.LastExceptionMessage != null)
                {
                    var message = $"Last error occurred at {viewModel.LastExceptionDate.ToString("g")}:\r\n{viewModel.LastExceptionMessage}";
                    await Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
                }
            }
            statusTooltip.IsVisible = true;
            await Task.Delay(3000);
            statusTooltip.IsVisible = false;
        }
    }

}
