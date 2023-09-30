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
        private DateTime _lastRefreshed;
        private Dictionary<long, VehicleData> _vehicles;

        public TeslaExtrasProvider()
        {
        }

        public async Task<string> RefreshStatus()
        {
            try
            {
                if (_vehicles == null)
                {
                    await GetVehicles();
                }

                var dataAge = DateTime.Now - _lastRefreshed;
                if (dataAge > TimeSpan.FromMinutes(15))
                {
                    await GetChargeLevels();
                }
                string message = "🚘 ";
                foreach (var v in _vehicles.Values)
                {
                    message += $"{v.VehicleName}: {v.BatteryLevel}%  ";
                }
                return message;
            }
            catch
            {
                return "Tesla failed";
            }
        }

        private async Task GetVehicles()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            _vehicles = new Dictionary<long, VehicleData>();
            foreach (var p in productsResponse["response"])
            {
                if (p["vehicle_id"] != null)
                {
                    var v = new VehicleData();
                    v.VehicleId = p["id"].Value<long>();
                    v.VehicleName = p["display_name"].Value<string>();
                    v.IsAwake = p["state"].Value<string>() == "online";
                    _vehicles.Add(v.VehicleId, v);
                }
            }
        }

        private async Task UpateOnlineStatus()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            _vehicles = new Dictionary<long, VehicleData>();
            foreach (var p in productsResponse["response"])
            {
                var id = p["id"].Value<long>();
                _vehicles[id].IsAwake = p["state"].Value<string>() == "online";
            }
        }

        private async Task GetChargeLevels()
        {
            foreach (var v in _vehicles.Values)
            {
                if (!v.IsAwake && (DateTime.Now - v.LastUpdated).TotalHours > 6)
                {
                    // Only wake up once every 6 hours
                    await WakeUpVehicle(v.VehicleId);
                    await Task.Delay(5);
                }

                if (v.IsAwake)
                {
                    var vehicleResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + v.VehicleId.ToString() + "/vehicle_data", null);
                    v.BatteryLevel = vehicleResponse["response"]["charge_state"]["battery_level"].Value<int>();
                    v.LastUpdated = DateTime.Now;
                }

            }    

            _lastRefreshed = DateTime.Now;
        }

        private async Task WakeUpVehicle(long id)
        {
            await ApiHelper.CallPostApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + id.ToString() + "/wake_up");
        }

        public class VehicleData
        {
            public long VehicleId { get; set; }
            public string VehicleName { get; set; }
            public int BatteryLevel { get; set; }
            public bool IsAwake { get; set; }
            public DateTime LastUpdated { get; set;  }
        }
    }
}
