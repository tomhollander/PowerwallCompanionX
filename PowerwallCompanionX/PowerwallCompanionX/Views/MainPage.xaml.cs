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
            try
            {
                var siteId = Application.Current.Properties[AppProperties.SiteId].ToString();

                var powerInfo = await ApiHelper.CallGetApiWithTokenRefresh($"{ApiHelper.BaseUrl}/api/1/energy_sites/{siteId}/live_status", "LiveStatus");

                viewModel.BatteryPercent = (powerInfo["response"]["energy_left"].Value<double>() / powerInfo["response"]["total_pack_energy"].Value<double>()) * 100D;
                viewModel.HomeValue = powerInfo["response"]["load_power"].Value<double>();
                viewModel.SolarValue = powerInfo["response"]["solar_power"].Value<double>();
                viewModel.BatteryValue = powerInfo["response"]["battery_power"].Value<double>();
                viewModel.GridValue = powerInfo["response"]["grid_power"].Value<double>();
                viewModel.GridActive = powerInfo["response"]["grid_status"].Value<string>() != "Inactive";
                viewModel.NotifyProperties();
                viewModel.StatusOK = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Application.Current.MainPage = new LoginPage();
                viewModel.LastExceptionMessage = ex.Message;
                viewModel.LastExceptionDate = DateTime.Now;
                viewModel.NotifyProperties();
                viewModel.StatusOK = false;
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
