using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App.Admin;
using Newtonsoft.Json.Linq;

namespace PowerwallCompanionX.Extras
{
    internal class TeslaExtrasProvider : IExtrasProvider
    {
        private DateTime _lastRefreshed;
        private Dictionary<string, VehicleData> _vehicles;
        private string lastMessage = "Tesla status pending";

        public TeslaExtrasProvider()
        {
        }

        public async Task<string> RefreshStatus()
        {
            try
            {
                var dataAge = DateTime.Now - _lastRefreshed;
                if (dataAge > TimeSpan.FromMinutes(15))
                {
                    // Only ever update once every 15 mins


                    if (_vehicles == null)
                    {
                        await GetVehicles();
                    }

                    await UpdateOnlineStatus();


                    if (_vehicles.Values.Any(v => !v.IsAwake))
                    {
                        if (dataAge > TimeSpan.FromHours(6))
                        {
                            // Only wake cars once every 6 hours
                            foreach (var v in _vehicles.Values)
                            {
                                if (!v.IsAwake)
                                {
                                    await WakeUpVehicle(v.VehicleId);
                                }
                            }
                        }
                        // Return previous status; wait until next cycle if the car has been woken
                        return lastMessage;
                    }

                    await GetChargeLevels();

                    lastMessage = "🚘 ";
                    foreach (var v in _vehicles.Values)
                    {
                        lastMessage += $"{v.VehicleName}: {v.BatteryLevel}%  ";
                    }
                }
                return lastMessage;
            }
            catch
            {
                return "Tesla status failed";
            }
        }

        private async Task GetVehicles()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            _vehicles = new Dictionary<string, VehicleData>();
            foreach (var p in productsResponse["response"])
            {
                if (p["vehicle_id"] != null)
                {
                    var v = new VehicleData();
                    v.VehicleId = p["id"].Value<string>();
                    v.VehicleName = p["display_name"].Value<string>();
                    v.IsAwake = p["state"].Value<string>() == "online";
                    _vehicles.Add(v.VehicleId, v);
                }
            }
        }

        private async Task UpdateOnlineStatus()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            foreach (var p in productsResponse["response"])
            {
                if (p["vehicle_id"] != null)
                {
                    var id = p["id"].Value<string>();
                    _vehicles[id].IsAwake = p["state"].Value<string>() == "online";
                }
            }
        }

        private async Task GetChargeLevels()
        {
            foreach (var v in _vehicles.Values)
            {
                if (v.IsAwake)
                {
                    var vehicleResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + v.VehicleId.ToString() + "/vehicle_data", null);
                    v.BatteryLevel = vehicleResponse["response"]["charge_state"]["battery_level"].Value<int>();
                    v.LastUpdated = DateTime.Now;
                }

            }    
            _lastRefreshed = DateTime.Now;
        }

        private async Task WakeUpVehicle(string id)
        {
            await ApiHelper.CallPostApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + id + "/wake_up");
        }

        public class VehicleData
        {
            public string VehicleId { get; set; }
            public string VehicleName { get; set; }
            public int BatteryLevel { get; set; }
            public bool IsAwake { get; set; }
            public DateTime LastUpdated { get; set;  }
        }
    }
}
