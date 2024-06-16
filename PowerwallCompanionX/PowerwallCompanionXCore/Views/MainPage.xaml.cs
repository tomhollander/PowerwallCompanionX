using Microsoft.Maui.Controls.PlatformConfiguration;
using Newtonsoft.Json.Linq;
using PowerwallCompanion.Lib;
using PowerwallCompanion.Lib.Models;
using PowerwallCompanionX.Extras;
using PowerwallCompanionX.Media;
using PowerwallCompanionX.ViewModels;
using Syncfusion.Maui.Core.Carousel;
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

        private IDispatcherTimer timer;
        private string lastOrientation;

        private Thickness timeDefaultMargin;

        private PowerwallApi powerwallApi;
        private IExtrasProvider extrasProvider;
        private TariffHelper tariffHelper;

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
            timer.Stop();
        }

        private async Task RefreshDataFromTeslaOwnerApi()
        {
            await RefreshTariffData(); // Refresh tariff data first, as it's used in other data refreshes

            var tasks = new List<Task>()
            {
                GetCurrentPowerData(),
                GetEnergyHistoryData(),
                GetPowerHistoryData()
            };
            await Task.WhenAll(tasks);

        }

        private async Task RefreshTariffData()
        {
            if (tariffHelper == null && Settings.ShowEnergyCosts)
            {
                try
                {
                    var ratePlan = await powerwallApi.GetRatePlan();
                    tariffHelper = new TariffHelper(ratePlan);
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
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
            }
            else if (viewModel.BatteryDay != (await powerwallApi.ConvertToPowerwallDate(DateTime.Now)).Date)
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
                    powerwallApi.GetEnergyTotalsForDay(DateTime.Now.Date.AddDays(-1), tariffHelper),
                    powerwallApi.GetEnergyTotalsForDay(DateTime.Now.Date, tariffHelper)
                };
                var results = await Task.WhenAll(tasks);
                viewModel.EnergyTotalsYesterday = results[0];
                viewModel.EnergyTotalsToday = results[1];

                viewModel.NotifyDailyEnergyProperties();

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

                viewModel.PowerChartSeries = await powerwallApi.GetPowerChartSeriesForLastTwoDays();

                DateTime d = await powerwallApi.ConvertToPowerwallDate(DateTime.Now);
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
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
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
            chart.WidthRequest = dailyEnergyGrid.Width;
            chart.MaximumHeightRequest = Math.Min(dailyEnergyGrid.Height/3, 300);
        }

    }

}
