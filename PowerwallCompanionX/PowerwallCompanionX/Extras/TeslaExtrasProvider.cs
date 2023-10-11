using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App.Admin;
using Newtonsoft.Json.Linq;

namespace PowerwallCompanionX.Extras
{
    internal class TeslaExtrasProvider : IExtrasProvider
    {
        private DateTime _lastProcessed;
        private Dictionary<string, VehicleData> _vehicles;
        private int _wakeTeslaHours;
        private string lastMessage = "Tesla status pending";
        private bool initialRefreshDone = false;

        public TeslaExtrasProvider(int wakeTeslaHours)
        {
            _wakeTeslaHours = wakeTeslaHours;
        }

        public async Task<string> RefreshStatus()
        {
            try
            { 
                // Only ever update once every 10 mins
                var timeSinceUpdate = DateTime.Now - _lastProcessed;
                if (timeSinceUpdate > TimeSpan.FromMinutes(10))
                {
                    _lastProcessed = DateTime.Now;
                    if (_vehicles == null)
                    {
                        await GetVehicles();
                    }
                    else
                    {
                        await UpdateOnlineStatus();
                    }

                    var tasks = new List<Task>();
                    foreach (var v in _vehicles.Values) // Run in parallel
                    {
                        tasks.Add(Task.Run(async () => 
                        {
                            if (v.IsAwake)
                            {
                                Debug.WriteLine(DateTime.Now + ": " + v.VehicleName + " is awake.");
                                await GetChargeLevel(v);
                            }
                            else
                            {
                                Debug.WriteLine(DateTime.Now + ": " + v.VehicleName + " is asleep.");
                                var timeSinceWoken = DateTime.Now - v.LastWoken;
                                if (!initialRefreshDone || (_wakeTeslaHours > 0 && timeSinceWoken.TotalHours > _wakeTeslaHours))
                                {
                                    await WakeUpVehicle(v);
                                    await WakeForVehicleToReportAsAwake(v);
                                    await GetChargeLevel(v);
                                }
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);
     
                    lastMessage = "🚘 ";
                    foreach (var v in _vehicles.Values)
                    {
                        lastMessage += $"{v.VehicleName}: {v.BatteryLevel}%  ";
                    }
                }
                initialRefreshDone = true;
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
            Debug.WriteLine(DateTime.Now + ": UpdateOnlineStatus");
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

        private async Task GetChargeLevel(VehicleData vehicle)
        {
            Debug.WriteLine(DateTime.Now + ": Getting charge level for " + vehicle.VehicleName);
            try
            {
                var vehicleResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + vehicle.VehicleId.ToString() + "/vehicle_data", null);
                vehicle.BatteryLevel = vehicleResponse["response"]["charge_state"]["battery_level"].Value<int>();
                vehicle.LastUpdated = DateTime.Now;
            }
            catch
            {
                Debug.WriteLine(DateTime.Now + ": Error getting charge status for " + vehicle.VehicleName);
                vehicle.IsAwake = false;
            }

        }

        private async Task WakeUpVehicle(VehicleData vehicle)
        {
            Debug.WriteLine(DateTime.Now + ": Waking " + vehicle.VehicleName);
            await ApiHelper.CallPostApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/vehicles/" + vehicle.VehicleId + "/wake_up");
            vehicle.LastWoken = DateTime.Now;
        }

        private async Task WakeForVehicleToReportAsAwake(VehicleData vehicle)
        {
            int attempts = 0;
            while (attempts++ < 5)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                await UpdateOnlineStatus();
                if (_vehicles[vehicle.VehicleId].IsAwake)
                {
                    return;
                }
            }
        }


        public class VehicleData
        {
            public string VehicleId { get; set; }
            public string VehicleName { get; set; }
            public int BatteryLevel { get; set; }
            public bool IsAwake { get; set; }
            public DateTime LastUpdated { get; set;  }
            public DateTime LastWoken { get; set; }
        }
    }
}
