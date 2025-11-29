using PowerwallCompanion.Lib;
using System.Text;
using System.Text.Json.Nodes;


namespace PowerwallCompanionX.Extras
{
    internal class PowerwallExtrasProvider : IExtrasProvider
    {
        private string gatewayIP;
        private string gatewayPassword;
        private const int warrantedCapacity = 13500;
        private static string lastStatus = null;
        private DateTime lastRefeshed = DateTime.MinValue;
        private TimeSpan refreshInterval = TimeSpan.FromDays(1);
        public PowerwallExtrasProvider(string gatewayIP, string gatewayPassword)
        {
            Telemetry.TrackEvent("PowerwallExtrasProvider initialised");
            this.gatewayIP = gatewayIP;
            this.gatewayPassword = gatewayPassword;
        }


        public async Task<string> RefreshStatus()
        {
            if (Settings.BatteryHealthMode == "Estimates")
            {
                return await RefreshStatusFromEstimates();
            }
            else
            {
                return await RefreshStatusFromGateway();
            }
        }

        public async Task<string> RefreshStatusFromEstimates()
        {
            if ((DateTime.Now - lastRefeshed) < refreshInterval)
            {
                return lastStatus ?? "🔋 Estimates unavailable";
            }

            try
            {
                var api = new PowerwallApi(Settings.SiteId, new MauiPlatformAdapter());
                var estimator = new BatteryCapacityEstimator(api);
                var siteInfo = await api.GetEnergySiteInfo();
                int batteryCount = siteInfo.NumberOfBatteries;
                double estimatedCapacity = await estimator.GetEstimatedBatteryCapacity(DateTime.Today);
                int totalWarantedCapacity = batteryCount * warrantedCapacity;
                int percentWarranted = (int)((estimatedCapacity / (double)totalWarantedCapacity) * 100);

                lastStatus = $"🔋 Est. Capacity: {estimatedCapacity / 1000.0:f2}kWh ({percentWarranted}%)";
                return lastStatus;
            }
            catch (Exception)
            {
                lastRefeshed = DateTime.Now; // Prevent frequent retries
                if (lastStatus == null)
                {
                    return "🔋 Estimates unavailable";
                }
                else
                {
                    return lastStatus + "*"; // Indicate stale data
                }
            }
        }

        public async Task<string> RefreshStatusFromGateway()
        {
            
            if ((DateTime.Now - lastRefeshed) < refreshInterval)
            {
                return lastStatus ?? "🔋 Gateway unreachable"; 
            }

            if (gatewayIP == null || gatewayPassword == null)
            {
                return "Configure gateway details in Settings";
            }

            try
            {
                var status = await GetStatus();
                int batteryCount = status["available_blocks"].GetValue<int>();
                int totalCapacity = status["nominal_full_pack_energy"].GetValue<int>();
                int totalWarantedCapacity = batteryCount * warrantedCapacity;
                int percentWarranted = (int)((totalCapacity / (double)totalWarantedCapacity) * 100);

                lastStatus = $"🔋 Capacity: {totalCapacity / 1000.0:f2}kWh ({percentWarranted}%)";
                return lastStatus;
            }
            catch (Exception)
            {
                lastRefeshed = DateTime.Now; // Prevent frequent retries
                if (lastStatus == null)
                {
                    return "🔋 Gateway unreachable";
                }
                else
                {
                    return lastStatus + "*"; // Indicate stale data
                }
            }

        }

        private async Task<JsonObject> GetStatus()
        {

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(10);
            var payload = $"{{\"username\":\"customer\",\"password\":\"{gatewayPassword}\", \"email\":\"me@example.com\",\"clientInfo\":{{\"timezone\":\"Australia/Sydney\"}}}}";
            var content = new StringContent(payload, new UTF8Encoding(), "application/json");
            var response = await client.PostAsync($"https://{gatewayIP}/api/login/Basic", content);
            var cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{gatewayIP}/api/system_status");
            foreach (var cookie in cookies)
            {
                request.Headers.Add("Cookie", cookie);
            }
            response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            lastRefeshed = DateTime.Now;
            return JsonNode.Parse(responseBody) as JsonObject;

        }
    }
}
