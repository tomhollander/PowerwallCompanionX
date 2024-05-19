using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace PowerwallCompanionX.Extras
{
    internal class TeslaExtrasProvider : IExtrasProvider
    {
        private DateTime _lastProcessed;
        private static Dictionary<string, VehicleData> _vehicles; // Static so we keep last known charge level if user opens different pages
        private string lastMessage = "Tesla status pending";
        private bool initialRefreshDone = false;

        public TeslaExtrasProvider()
        {
            Analytics.TrackEvent("TeslaExtrasProvider initialised");
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

                    foreach (var v in _vehicles.Values) 
                    {
                        await GetChargeLevel(v);
                    }
     
                    lastMessage = "🚘 ";
                    foreach (var v in _vehicles.Values)
                    {
                        if (v.IsAwake || v.LastUpdated > DateTime.MinValue)
                        {
                            lastMessage += $"{v.VehicleName}: {v.BatteryLevel}%  ";
                        }
                        else
                        {
                            lastMessage += $"{v.VehicleName}: 💤  ";
                        }
                    }
                }
                initialRefreshDone = true;
                return lastMessage;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return "Tesla status failed";
            }
        }

        private async Task GetVehicles()
        {
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh("/api/1/products", null);
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
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh("/api/1/products", null);
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
                var vehicleResponse = await ApiHelper.CallGetApiWithTokenRefresh("/api/1/vehicles/" + vehicle.VehicleId.ToString() + "/vehicle_data", null);
                vehicle.BatteryLevel = vehicleResponse["response"]["charge_state"]["battery_level"].Value<int>();
                vehicle.LastUpdated = DateTime.Now;
            }
            catch 
            {
                Debug.WriteLine(DateTime.Now + ": Error getting charge status for " + vehicle.VehicleName);
                vehicle.IsAwake = false;
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
