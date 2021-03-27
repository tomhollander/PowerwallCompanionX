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

                viewModel.BatteryPercent = (powerInfo["response"]["energy_left"].Value<double>() / powerInfo["response"]["total_pack_energy"].Value<double>()) * 100D;
                viewModel.HomeValue = powerInfo["response"]["load_power"].Value<double>();
                viewModel.SolarValue = powerInfo["response"]["solar_power"].Value<double>();
                viewModel.BatteryValue = powerInfo["response"]["battery_power"].Value<double>();
                viewModel.GridValue = powerInfo["response"]["grid_power"].Value<double>();
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
                var json = await ApiHelper.CallGetApiWithTokenRefresh($"{ApiHelper.BaseUrl}/api/1/energy_sites/{Settings.SiteId}/history?kind=energy&period={period}", "EnergyHistory" + period);

                var yesterday = json["response"]["time_series"][0];
                viewModel.HomeEnergyYesterday = yesterday["consumer_energy_imported_from_grid"].Value<double>() + yesterday["consumer_energy_imported_from_solar"].Value<double>() + yesterday["consumer_energy_imported_from_battery"].Value<double>()+ yesterday["consumer_energy_imported_from_generator"].Value<double>();
                viewModel.SolarEnergyYesterday = yesterday["solar_energy_exported"].Value<double>();
                viewModel.GridEnergyImportedYesterday = yesterday["grid_energy_imported"].Value<double>();
                viewModel.GridEnergyExportedYesterday = yesterday["grid_energy_exported_from_solar"].Value<double>() + yesterday["grid_energy_exported_from_battery"].Value<double>();
                viewModel.BatteryEnergyImportedYesterday = yesterday["battery_energy_imported_from_grid"].Value<double>() + yesterday["battery_energy_imported_from_solar"].Value<double>();
                viewModel.BatteryEnergyExportedYesterday = yesterday["battery_energy_exported"].Value<double>();

                var today = json["response"]["time_series"][1];
                viewModel.HomeEnergyToday = today["consumer_energy_imported_from_grid"].Value<double>() + today["consumer_energy_imported_from_solar"].Value<double>() + today["consumer_energy_imported_from_battery"].Value<double>() + today["consumer_energy_imported_from_generator"].Value<double>();
                viewModel.SolarEnergyToday = today["solar_energy_exported"].Value<double>();
                viewModel.GridEnergyImportedToday = today["grid_energy_imported"].Value<double>();
                viewModel.GridEnergyExportedToday = today["grid_energy_exported_from_solar"].Value<double>() + today["grid_energy_exported_from_battery"].Value<double>();
                viewModel.BatteryEnergyImportedToday = today["battery_energy_imported_from_grid"].Value<double>() + today["battery_energy_imported_from_solar"].Value<double>();
                viewModel.BatteryEnergyExportedToday = today["battery_energy_exported"].Value<double>();

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
    }

}
