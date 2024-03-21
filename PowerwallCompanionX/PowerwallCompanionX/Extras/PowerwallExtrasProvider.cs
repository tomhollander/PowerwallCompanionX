using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Android.App.Assist.AssistStructure;

namespace PowerwallCompanionX.Extras
{
    internal class PowerwallExtrasProvider : IExtrasProvider
    {
        private MainViewModel _viewModel;
        private int batteryCount;
        private const double warrantedCapacity = 13500;
        public PowerwallExtrasProvider(MainViewModel viewModel)
        {
            Analytics.TrackEvent("PowerwallExtrasProvider initialised");
            _viewModel = viewModel;
        }

        private async Task<int> GetBatteryCount()
        {
            var siteInfoJson = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{Settings.SiteId}/site_info", "SiteInfo");
            return siteInfoJson["response"]["battery_count"].Value<int>();
        }

        public async Task<string> RefreshStatus()
        {
            try
            {
                if (batteryCount == 0)
                {
                    batteryCount = await GetBatteryCount();
                }
                if (_viewModel.TotalPackEnergy == 0)
                {
                    return "Capacity unavailable, blame Tesla 😡";
                }

                return $"🔋 Capacity: {_viewModel.TotalPackEnergy / 1000:f2}kWh ({(_viewModel.TotalPackEnergy / (warrantedCapacity * batteryCount) * 100):f0}%)";
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return "Powerwall status failed";
            }

        }
    }
}
