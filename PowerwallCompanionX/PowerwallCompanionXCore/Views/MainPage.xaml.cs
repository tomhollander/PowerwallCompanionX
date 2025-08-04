using PowerwallCompanion.Lib;
using PowerwallCompanion.Lib.Models;
using PowerwallCompanionX.Extras;
using PowerwallCompanionX.Media;
using PowerwallCompanionX.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace PowerwallCompanionX.Views
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel viewModel;
        private readonly TimeSpan liveStatusRefreshInterval = new TimeSpan(0, 0, 30);
        private readonly TimeSpan energyHistoryRefreshInterval = new TimeSpan(0, 5, 0);
        private readonly TimeSpan powerHistoryRefreshInterval = new TimeSpan(0, 5, 0);
        private readonly TimeSpan energySiteInfoRefreshInterval = new TimeSpan(1, 0, 0);

        private DateTime lastManualSwipe;
        private readonly TimeSpan swipeIdlePeriod = new TimeSpan(0, 1, 0);
        private int noDataResponseCount = 0;

        private double maxPercentSinceSound = 0D;
        private double minPercentSinceSound = 100D;

        private IDispatcherTimer timer;
        private string lastOrientation;

        private Thickness timeDefaultMargin;

        private PowerwallApi powerwallApi;
        private IExtrasProvider extrasProvider;
        private ITariffProvider tariffHelper;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            powerwallApi = new PowerwallApi(Settings.SiteId, new MauiPlatformAdapter());
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
            }
            else
            {
                rootGrid.Children.Remove(mainGrid);
                rootGrid.Children.Remove(dailyEnergyGrid);
                tabletGrid.Children.Add(mainGrid);
                tabletGrid.Children.Add(dailyEnergyGrid);
                settingsButton.Opacity = 1;
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
                    viewModel.BarChartMaxWidth = Width - 50;
                    SetPortraitMode(mainGrid, dailyEnergyGrid);
                }
                else if (visualState == "Landscape")
                {
                    // Restore default margins for clock 
                    timeTextBlock.Margin = timeDefaultMargin;
                    viewModel.BarChartMaxWidth = Width - 300;
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
                    viewModel.BarChartMaxWidth = Width;
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
                    viewModel.BarChartMaxWidth = (Width / 2) - 50;
                }
            }


            viewModel.PageWidth = Width;
            viewModel.PageHeight = Height;
            viewModel.RecalculatePageMargin();
            viewModel.NotifyPropertyChanged(nameof(viewModel.BarChartMaxWidth));
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
                        page2Grid.SetRow(chartContainer, 3);
                        page2Grid.SetColumnSpan(chartContainer, 5);
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
                        page2Grid.SetRow(chartContainer, 6);
                        page2Grid.SetColumnSpan(chartContainer, 3);
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

            timer = Application.Current.Dispatcher.CreateTimer();
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
                
            };

            timer.Start();

        }

        protected override void OnDisappearing()
        {
            timer?.Stop();
        }

        private async Task RefreshDataFromTeslaOwnerApi()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                viewModel.LastExceptionMessage = "No internet access";
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.Status = MainViewModel.StatusEnum.Error;
                viewModel.NotifyPropertyChanged(nameof(viewModel.Time));
                return;
            }

            await GetInstallationTimeZoneIfNeeded(); // Usually a no-op
            await RefreshTariffData(); // Refresh tariff data first, as it's used in other data refreshes

            var tasks = new List<Task>()
            {
                GetCurrentPowerData(),
                GetEnergyHistoryData(),
                GetPowerHistoryData(),
                GetEnergySiteInfo()
            };
            await Task.WhenAll(tasks);

        }

        private async Task GetInstallationTimeZoneIfNeeded()
        {
            if (Settings.InstallationTimeZone == null)
            {
                try
                {
                    await powerwallApi.StoreInstallationTimeZone();
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex);
                }
            }
        }

        private async Task GetEnergySiteInfo()
        {
            try
            {
                if (viewModel.EnergySiteInfo == null || (DateTime.Now - viewModel.EnergySiteInfoLastRefreshed > energySiteInfoRefreshInterval))
                {
                    viewModel.EnergySiteInfo = await powerwallApi.GetEnergySiteInfo();
                    viewModel.EnergySiteInfoLastRefreshed = DateTime.Now;
                    viewModel.NotifyPropertyChanged(nameof(viewModel.EnergySiteInfo));
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
            }
        }

        private async Task RefreshTariffData()
        {
            if (tariffHelper == null && Settings.ShowEnergyCosts)
            {
                try
                {
                    var ratePlan = await powerwallApi.GetRatePlan();
                    tariffHelper = new TeslaRatePlanTariffProvider(ratePlan, Settings.DailySupplyCharge, 0.0M);
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex);
                }
            }
            
        }

        private async Task GetCurrentPowerData()
        { 
            try
            {
                if (DateTime.Now - viewModel.LiveStatusLastRefreshed < liveStatusRefreshInterval)
                {
                    return;
                }

                viewModel.InstantaneousPower = await powerwallApi.GetInstantaneousPower();
                await UpdateMinMaxPercentToday();
                viewModel.LiveStatusLastRefreshed = DateTime.Now;
                viewModel.Status = viewModel.InstantaneousPower.GridActive ? MainViewModel.StatusEnum.Online : MainViewModel.StatusEnum.GridOutage;
                viewModel.NotifyPowerProperties();


                PlaySoundsOnBatteryStatus(viewModel.InstantaneousPower.BatteryStoragePercent);
            }
            catch (UnauthorizedAccessException ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!(Application.Current.Windows[0].Page is LoginPage))
                    {
                        Application.Current.Windows[0].Page = new LoginPage();
                        viewModel.LastExceptionMessage = ex.Message;
                        viewModel.LastExceptionDate = DateTime.Now;
                        viewModel.NotifyPowerProperties();
                        viewModel.Status = MainViewModel.StatusEnum.Error;
                    }
                });
                
            }
            catch (NoDataException ex)
            {
                noDataResponseCount++;
                if (noDataResponseCount > 5)
                {
                    viewModel.LastExceptionMessage = ex.Message;
                    viewModel.LastExceptionDate = DateTime.Now;
                    viewModel.NotifyPowerProperties();
                    viewModel.Status = MainViewModel.StatusEnum.Error;
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyPowerProperties();
                viewModel.Status = MainViewModel.StatusEnum.Error;
            }
        }

        private async Task UpdateMinMaxPercentToday()
        {
            if (viewModel.InstantaneousPower == null)
            {
                return;
            }
            if (viewModel.BatteryDay == DateTime.MinValue)
            {                
                var minMax = await powerwallApi.GetBatteryMinMaxToday();
                viewModel.MinBatteryPercentToday = minMax.Item1;
                viewModel.MaxBatteryPercentToday = minMax.Item2;
                viewModel.BatteryDay = DateTime.Today;
            }
            else if (viewModel.BatteryDay != (powerwallApi.ConvertToPowerwallDate(DateTime.Now)).Date)
            {
                viewModel.BatteryDay = DateTime.Today;
                viewModel.MinBatteryPercentToday = viewModel.InstantaneousPower.BatteryStoragePercent;
                viewModel.MaxBatteryPercentToday = viewModel.InstantaneousPower.BatteryStoragePercent;
            }
            else if (viewModel.InstantaneousPower.BatteryStoragePercent < viewModel.MinBatteryPercentToday)
            {
                viewModel.MaxBatteryPercentToday = viewModel.InstantaneousPower.BatteryStoragePercent;
            }
            else if (viewModel.InstantaneousPower.BatteryStoragePercent > viewModel.MaxBatteryPercentToday)
            {
                viewModel.MaxBatteryPercentToday = viewModel.InstantaneousPower.BatteryStoragePercent;
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

                var tasks = new List<Task<EnergyTotals>>()
                {
                    powerwallApi.GetEnergyTotalsForDay(-1, tariffHelper),
                    powerwallApi.GetEnergyTotalsForDay(0, tariffHelper)
                };
                var results = await Task.WhenAll(tasks);
                if (!EnergyTotalsAllZeros(results[0]))
                {
                    viewModel.EnergyTotalsYesterday = results[0];
                    viewModel.EnergyHistoryLastRefreshed = DateTime.Now;
                }
                if (!EnergyTotalsAllZeros(results[1]))
                {
                    viewModel.EnergyTotalsToday = results[1];
                    viewModel.EnergyHistoryLastRefreshed = DateTime.Now;
                }

                viewModel.NotifyDailyEnergyProperties();

            }
            catch (NoDataException)
            {
                // Don't update the energy history if there is a no data response
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyDailyEnergyProperties();
            }
        }

        private bool EnergyTotalsAllZeros(EnergyTotals energyTotals)
        {
            // Sometimes the Tesla API returns what looks like a valid response, but with all values as zero
            if (energyTotals == null)
            {
                return true;
            }
            if (energyTotals.SolarEnergy == 0 && energyTotals.BatteryEnergyCharged == 0 && energyTotals.BatteryEnergyDischarged == 0 &&
                energyTotals.GridEnergyExported == 0 && energyTotals.GridEnergyImported == 0 && energyTotals.HomeEnergy == 0)
            {
                return true;
            }
            return false;
        }

        public async Task GetPowerHistoryData()
        {
            try
            {
                if (DateTime.Now - viewModel.PowerHistoryLastRefreshed < powerHistoryRefreshInterval || !Settings.ShowGraph)
                {
                    return;
                }

                viewModel.PowerChartSeries = await powerwallApi.GetPowerChartSeriesForLastTwoDays();

                DateTime d = powerwallApi.ConvertToPowerwallDate(DateTime.Now);
                if (Settings.AccessToken == "DEMO")
                {
                    viewModel.ChartMaxDate = new DateTime(2018, 3, 1);
                }
                else
                {
                    viewModel.ChartMaxDate = d.Date.AddDays(1);
                }


                viewModel.PowerHistoryLastRefreshed = DateTime.Now;
                viewModel.NotifyGraphProperties();
            }
            catch (NoDataException)
            {
                // Don't update the power history if there is a no data response
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.LastExceptionMessage = ex.Message;
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
                    await Application.Current.Windows[0].Page.DisplayAlert("Alert", message, "OK");
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
            chart.WidthRequest = dailyEnergyGrid.Width;
            chart.MaximumHeightRequest = Math.Min(dailyEnergyGrid.Height/3, 300);
        }

        private async void ErrorGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Last error", viewModel.LastExceptionMessage, "OK");
        }
    }

}
