using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;


namespace PowerwallCompanionX.Extras
{
    internal class PowerwallExtrasProvider : IExtrasProvider
    {
        private string gatewayIP;
        private string gatewayPassword;
        private const int warrantedCapacity = 13500;
        private static string lastStatus = null;
        private DateTime lastRefeshed = DateTime.MinValue;
        private TimeSpan refreshInterval = TimeSpan.FromMinutes(20);
        public PowerwallExtrasProvider(string gatewayIP, string gatewayPassword)
        {
            Analytics.TrackEvent("PowerwallExtrasProvider initialised");
            this.gatewayIP = gatewayIP;
            this.gatewayPassword = gatewayPassword;
        }


        public async Task<string> RefreshStatus()
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
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
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
