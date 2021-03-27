using Newtonsoft.Json.Linq;
using PowerwallCompanionX.ViewModels;
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
        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            this.BindingContext = viewModel;

            Task.Run(() => RefreshData());
        }

        private async Task RefreshData()
        { 
            await RefreshDataFromTeslaOwnerApi();

            Device.StartTimer(TimeSpan.FromSeconds(30), () =>
            {
                RefreshDataFromTeslaOwnerApi();
                return true; // True = Repeat again, False = Stop the timer
            });

        }

        private async Task RefreshDataFromTeslaOwnerApi()
        {
            await GetCurrentPowerData();
            await GetEnergyHistoryData();
        }

        private async Task GetCurrentPowerData()
        { 
            try
            {
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
                viewModel.NotifyProperties();
                viewModel.StatusOK = true; 
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
                        viewModel.StatusOK = false;
                    }
                });
                
            }
            catch (Exception ex)
            {
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyProperties();
                viewModel.StatusOK = false;
            }
        }

        private async Task GetEnergyHistoryData()
        {
            try
            {
                viewModel.StatusOK = true;
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

                viewModel.NotifyProperties();
                viewModel.StatusOK = true;

            }
            catch (Exception ex)
            {
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyProperties();
                viewModel.StatusOK = false;
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
            if (args.Index == 0)
            {
                unitsLabel.Text = "kW";
            }
            else if (args.Index == 1)
            {
                unitsLabel.Text = "kWh";
            }
        }
    }

}
