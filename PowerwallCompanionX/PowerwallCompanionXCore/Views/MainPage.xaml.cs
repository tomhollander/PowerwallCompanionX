using Newtonsoft.Json.Linq;
using PowerwallCompanionX.Extras;
using PowerwallCompanionX.Media;
using PowerwallCompanionX.ViewModels;
using System.Text;

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
        private TariffHelper tariffHelper;

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
                    extrasProvider = new PowerwallExtrasProvider(Settings.GatewayIP, Settings.GatewayPassword);
                    break;
                case "Weather":
                    extrasProvider = new WeatherExtrasProvider(Settings.WeatherCity, Settings.WeatherUnits);
                    break;
                case "Tariffs":
                    extrasProvider = new EnergyTariffExtrasProvider(viewModel);
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
                //carousel.IsVisible = true; //x
            }
            else
            {
                rootGrid.Children.Remove(mainGrid);
                rootGrid.Children.Remove(dailyEnergyGrid);
                tabletGrid.Children.Add(mainGrid);
                tabletGrid.Children.Add(dailyEnergyGrid);
                settingsButton.Opacity = 1;
                //carousel.IsVisible = false; //x
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

            if (Height > 0 && Height < 800)
            {
                double proposedHeight = Height / 4;
                if (proposedHeight < 150)
                {
                    chart.IsVisible = false;
                }
                else
                {
                    chart.IsVisible = true;
                    chart.MaximumHeightRequest = proposedHeight;
                }
                
            }

            viewModel.PageWidth = Width;
            viewModel.PageHeight = Height;
            viewModel.RecalculatePageMargin();
        }

        private void SetLandscapeMode(Grid page1Grid, Grid page2Grid)
        {
            if (lastOrientation == "Landscape") return;

            // Page 1 : Move graph from row to column
            page1Grid.RowDefinitions[0].Height = GridLength.Auto;
            page1Grid.RowDefinitions[1].Height = new GridLength(0);
            page1Grid.ColumnDefinitions[0].Width = new GridLength(330);
            page1Grid.ColumnDefinitions[1].Width = GridLength.Auto;
            page1Grid.SetRow(page1Grid.Children[1], 0);
            page1Grid.SetColumn(page1Grid.Children[1], 1);

            // Page 2 : Transpose grid
            page2Grid.ColumnDefinitions.Clear();
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            for (int i = 0; i < 4; i++)
            {
                page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            }
            page2Grid.RowDefinitions.Clear();
            //for (int i = 0; i < 4; i++)
            //{
            //    page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            //}
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = Settings.ShowGraph ? GridLength.Star : new GridLength(0)});

            var chart = page2Grid.Children.Last();
            if (page2Grid.GetRow(chart) == 6)
            {
                // Previously in portrait mode, so transpose
                foreach (var item in page2Grid.Children)
                {
                    if (item != chart)
                    {
                        int currentRow = page2Grid.GetRow(item);
                        int currentColumn = page2Grid.GetColumn(item);
                        page2Grid.SetColumn(item, currentRow);
                        page2Grid.SetRow(item, currentColumn);
                    }
                    else
                    {
                        page2Grid.SetRow(chart, 3);
                        page2Grid.SetColumnSpan(chart, 5);
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
            page1Grid.SetRow(page1Grid.Children[1], 1);
            page1Grid.SetColumn(page1Grid.Children[1], 0);

            // Page 2 : Transpose grid
            page2Grid.RowDefinitions.Clear();
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            for (int i = 0; i < 4; i++)
            {
                page2Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            // Give more room for the grid power
            //page2Grid.RowDefinitions[3].Height = new GridLength(6, GridUnitType.Star);

            // Chart row
            page2Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            page2Grid.ColumnDefinitions.Clear();
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            page2Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            var chart = page2Grid.Children.Last();
            if (page2Grid.GetRow(chart) == 3)
            {
                // Previously in landscape mode, so transpose
                foreach (var item in page2Grid.Children)
                {
                    if (item != chart)
                    {
                        int currentRow = page2Grid.GetRow(item);
                        int currentColumn = page2Grid.GetColumn(item);
                        page2Grid.SetColumn(item, currentRow);
                        page2Grid.SetRow(item, currentColumn);
                    }
                    else
                    {
                        page2Grid.SetRow(chart, 6);
                        page2Grid.SetColumnSpan(chart, 3);
                    }
                }
            }
            lastOrientation = "Portrait";
        }

        private async Task RefreshData()
        {
            extrasTextBlock.IsVisible = (extrasProvider != null);
            await RefreshDataFromTeslaOwnerApi();
            viewModel.ExtrasContent = await extrasProvider?.RefreshStatus();
            int count = 0;

            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += (s, e) =>
            {
                Task.Run(async () =>
                {
                    await RefreshDataFromTeslaOwnerApi();
                    viewModel.ExtrasContent = await extrasProvider?.RefreshStatus();
                    if (++count > 6)
                    {
                        count = 0;
                        viewModel.RecalculatePageMargin();
                    }

                });
                if (ShowTwoPages() &&
                    Settings.CyclePages && DateTime.Now - lastManualSwipe > swipeIdlePeriod)
                {
                    carousel.SelectedIndex = (carousel.SelectedIndex + 1) % 2;
                }
                //x
                //return keepRefreshing; // True = Repeat again, False = Stop the timer
            };

            timer.Start();

        }

        protected override void OnDisappearing()
        {
            keepRefreshing = false;
        }

        private async Task RefreshDataFromTeslaOwnerApi()
        {
            // Doing these in parallel seems to break stuff
            //await GetCurrentPowerData();
            //await GetEnergyHistoryData();
            //await GetPowerHistoryData();

            var tasks = new List<Task>()
            {
                GetCurrentPowerData(),
                GetEnergyHistoryData(),
                GetPowerHistoryData()
            };
            await Task.WhenAll(tasks);

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

                viewModel.BatteryPercent = GetJsonDoubleValue(powerInfo["response"]["percentage_charged"]);
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

                var tasks = new List<Task<JObject>>()
                {
                    GetCalendarHistoryData(DateTime.Now.Date.AddDays(-1)),
                    GetCalendarHistoryData(DateTime.Now.Date)
                };
                var results = await Task.WhenAll(tasks);
                var yesterdayEnergy = results[0];
                var todayEnergy = results[1];

                viewModel.HomeEnergyYesterday = 0;
                viewModel.SolarEnergyYesterday = 0;
                viewModel.GridEnergyImportedYesterday = 0;
                viewModel.GridEnergyExportedYesterday = 0;
                viewModel.BatteryEnergyImportedYesterday = 0;
                viewModel.BatteryEnergyExportedYesterday = 0;
                foreach (var period in yesterdayEnergy["response"]["time_series"])
                {
                    viewModel.HomeEnergyYesterday += GetJsonDoubleValue(period["total_home_usage"]);
                    viewModel.SolarEnergyYesterday += GetJsonDoubleValue(period["total_solar_generation"]);
                    viewModel.GridEnergyImportedYesterday += GetJsonDoubleValue(period["grid_energy_imported"]);
                    viewModel.GridEnergyExportedYesterday += GetJsonDoubleValue(period["grid_energy_exported_from_solar"]) + GetJsonDoubleValue(period["grid_energy_exported_from_generator"]) + GetJsonDoubleValue(period["grid_energy_exported_from_battery"]);
                    viewModel.BatteryEnergyImportedYesterday += GetJsonDoubleValue(period["battery_energy_imported_from_grid"]) + GetJsonDoubleValue(period["battery_energy_imported_from_solar"]) + GetJsonDoubleValue(period["battery_energy_imported_from_generator"]);
                    viewModel.BatteryEnergyExportedYesterday += GetJsonDoubleValue(period["battery_energy_exported"]);
                }

                viewModel.HomeEnergyToday = 0;
                viewModel.SolarEnergyToday = 0;
                viewModel.GridEnergyImportedToday = 0;
                viewModel.GridEnergyExportedToday = 0;
                viewModel.BatteryEnergyImportedToday = 0;
                viewModel.BatteryEnergyExportedToday = 0;
                foreach (var period in todayEnergy["response"]["time_series"])
                {
                    viewModel.HomeEnergyToday += GetJsonDoubleValue(period["total_home_usage"]);
                    viewModel.SolarEnergyToday += GetJsonDoubleValue(period["total_solar_generation"]);
                    viewModel.GridEnergyImportedToday += GetJsonDoubleValue(period["grid_energy_imported"]);
                    viewModel.GridEnergyExportedToday += GetJsonDoubleValue(period["grid_energy_exported_from_solar"]) + GetJsonDoubleValue(period["grid_energy_exported_from_generator"]) + GetJsonDoubleValue(period["grid_energy_exported_from_battery"]);
                    viewModel.BatteryEnergyImportedToday += GetJsonDoubleValue(period["battery_energy_imported_from_grid"]) + GetJsonDoubleValue(period["battery_energy_imported_from_solar"]) + GetJsonDoubleValue(period["battery_energy_imported_from_generator"]);
                    viewModel.BatteryEnergyExportedToday += GetJsonDoubleValue(period["battery_energy_exported"]);
                }

                viewModel.EnergyHistoryLastRefreshed = DateTime.Now;
                viewModel.NotifyDailyEnergyProperties();

                if (Settings.ShowEnergyCosts)
                {
                    await RefreshEnergyCostData(yesterdayEnergy, todayEnergy);
                }

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

        private async Task RefreshEnergyCostData(JObject yesterdayEnergy, JObject todayEnergy)
        {
            try
            {
                if (!Settings.ShowEnergyCosts)
                {
                    return;
                }
                if (tariffHelper == null)
                {
                    var ratePlan = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{Settings.SiteId}/tariff_rate", "TariffRate");
                    tariffHelper = new TariffHelper(ratePlan);
                }

                var yesterdayCost = tariffHelper.GetEnergyCostAndFeedInFromEnergyHistory((JArray)yesterdayEnergy["response"]["time_series"]);
                viewModel.EnergyCostYesterday = yesterdayCost.Item1;
                viewModel.EnergyFeedInYesterday = yesterdayCost.Item2;

                var todayCost = tariffHelper.GetEnergyCostAndFeedInFromEnergyHistory((JArray)todayEnergy["response"]["time_series"]);
                viewModel.EnergyCostToday = todayCost.Item1;
                viewModel.EnergyFeedInToday = todayCost.Item2;

                Analytics.TrackEvent("Energy cost data refreshed");
                viewModel.NotifyEnergyCostProperties();

            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        private async Task<JObject> GetCalendarHistoryData(DateTime date)
        {
            var url = GetCalendarHistoryUrl("energy", "day", date, date.AddDays(1).AddSeconds(-1));
            return await ApiHelper.CallGetApiWithTokenRefresh(url, "CalendarHistory");
        }

        private string GetCalendarHistoryUrl(string kind, string period, DateTime periodStart, DateTime periodEnd)
        {
            var sb = new StringBuilder();
            var siteId = Settings.SiteId;

            var timeZone = TimeZoneInfo.Local.Id;;
            var startOffset = TimeZoneInfo.FindSystemTimeZoneById(timeZone).GetUtcOffset(periodStart);
            var endOffset = TimeZoneInfo.FindSystemTimeZoneById(timeZone).GetUtcOffset(periodEnd);
            var startDate = new DateTimeOffset(periodStart, startOffset);
            var endDate = new DateTimeOffset(periodEnd, endOffset).AddSeconds(-1);

            sb.Append($"/api/1/energy_sites/{siteId}/calendar_history?");
            sb.Append("kind=" + kind);
            sb.Append("&period=" + period.ToLowerInvariant());
            if (period != "Lifetime")
            {
                sb.Append("&start_date=" + Uri.EscapeDataString(startDate.ToString("o")));
                sb.Append("&end_date=" + Uri.EscapeDataString(endDate.ToString("o")));
            }
            sb.Append("&time_zone=" + Uri.EscapeDataString(timeZone));
            sb.Append("&fill_telemetry=0");
            return sb.ToString();
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

        private void dailyEnergyGrid_SizeChanged(object sender, EventArgs e)
        {
            // Fix resize jitter on chart
            chart.WidthRequest = dailyEnergyGrid.Width;
        }
    }

}
