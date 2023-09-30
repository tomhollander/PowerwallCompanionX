using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PowerwallCompanionX.Extras
{
    internal class TeslaExtrasProvider : IExtrasProvider
    {
        private string _vehicleId;
        public string _vehicleName;
        private DateTime _lastRefreshed;
        private int _lastBatteryLevel;

        public TeslaExtrasProvider(string vehicleId)
        {
            _vehicleId = vehicleId;
        }

        public async Task<string> RefreshStatus()
        {
            try
            {
                var dataAge = DateTime.Now - _lastRefreshed;
                if (dataAge > TimeSpan.FromMinutes(15))
                {
                    await GetChargeLevel();
                }
                return $"{_vehicleName}: {_lastBatteryLevel}%";
            }
            catch
            {
                return "Tesla failed";
            }
        }
        
        // Don't think we need to use this but keeping it here just in case...
        private async Task<bool> IsVehicleAwake()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            var vehicle = productsResponse["response"].Where(r => r.Value<string>("id") == _vehicleId).FirstOrDefault();
            if (vehicle == null)
            {
                throw new KeyNotFoundException();
            }
            _vehicleName = vehicle["display_name"].Value<string>();
            return vehicle["state"].Value<string>() == "online";
        }


        private async Task WakeUpVehicle()
        {
            await ApiHelper.CallPostApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + _vehicleId + "/wake_up");
        }


        private async Task GetChargeLevel()
        {
            var vehicleResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + _vehicleId + "/vehicle_data", null);
            _lastBatteryLevel = vehicleResponse["response"]["charge_state"]["battery_level"].Value<int>();
            _vehicleName = vehicleResponse["response"]["vehicle_state"]["vehicle_name"].Value<string>();
            _lastRefreshed = DateTime.Now;
        }
    }
}
