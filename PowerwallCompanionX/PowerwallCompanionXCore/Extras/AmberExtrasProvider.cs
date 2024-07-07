using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace PowerwallCompanionX.Extras
{
    internal class AmberExtrasProvider : IExtrasProvider
    {
        private string _apiKey;
        private string _siteId;
        private DateTime _lastUpdated;
        private decimal _sellPrice;
        private decimal _buyPrice;
        private decimal _renewables;
        private const string baseUrl = "https://api.amber.com.au/v1";

        public AmberExtrasProvider(string apiKey)
        {
            _apiKey = apiKey;
            Telemetry.TrackEvent("AmberExtrasProvider initialised");
        }
        public async Task<string> RefreshStatus()
        {
            try
            {
                if (_siteId == null)
                {
                    await GetSiteId();
                }
                var dataAge = DateTime.Now - _lastUpdated;
                if (dataAge > TimeSpan.FromMinutes(5))
                {
                    await GetPrices();
                }
                string symbol = _sellPrice > 40 ? "🔴" : _sellPrice > 20 ? "🟡" : _sellPrice > 0 ? "🟢" : "💥";
                return $"⚡{symbol}{_sellPrice:f1}c ☀️ {_buyPrice:f1}c 🌱{_renewables:f0}%";
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                return "Amber failed";
            }
        }

        private async Task GetPrices()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await client.GetAsync(baseUrl + "/sites/" + _siteId + "/prices/current");
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseJson = JsonObject.Parse(responseMessage);
            foreach (var item in responseJson.AsArray())
            {
                if (item["channelType"].GetValue<string>() == "general")
                {
                    _sellPrice = item["perKwh"].GetValue<decimal>();
                    _renewables = item["renewables"].GetValue<decimal>();
                }
                else if (item["channelType"].GetValue<string>() == "feedIn")
                {
                    _buyPrice = -item["perKwh"].GetValue<decimal>();
                }
            }
            _lastUpdated = DateTime.Now;
        }

        private async Task GetSiteId()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await client.GetAsync(baseUrl + "/sites");
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseJson = JsonObject.Parse(responseMessage);
            foreach (var site in responseJson.AsArray())
            {
                if (site["status"].GetValue<string>() == "active")
                {
                    _siteId = site["id"].GetValue<string>();
                }
            }

        }
    }
}
