﻿using PowerwallCompanion.Lib;
using PowerwallCompanion.Lib.Models;

namespace PowerwallCompanionX.Extras
{
    // This is no longer available due to the API pricing changes, but keeping the code just in case
    internal class TeslaExtrasProvider : IExtrasProvider
    {
        private DateTime _lastProcessed;
        private VehicleApi vehicleApi;
        private static Dictionary<string, VehicleData> _vehicles; // Static so we keep last known charge level if user opens different pages
        private string lastMessage = "Tesla status pending";

        public TeslaExtrasProvider()
        {
            Telemetry.TrackEvent("TeslaExtrasProvider initialised");
            vehicleApi = new VehicleApi(new MauiPlatformAdapter());
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
                        _vehicles = await vehicleApi.GetVehicles();
                    }
                    else
                    {
                        await vehicleApi.UpdateOnlineStatus(_vehicles);
                    }

                    foreach (var v in _vehicles.Values)
                    {
                        await vehicleApi.UpdateChargeLevel(v);
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
                return lastMessage;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                return "Tesla status failed";
            }
        }

    }
}
