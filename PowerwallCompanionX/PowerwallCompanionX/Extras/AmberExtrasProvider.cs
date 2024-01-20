using Android.OS;
using Java.Net;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;

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
            Analytics.TrackEvent("AmberExtrasProvider initialised");
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
                Crashes.TrackError(ex);
                return "Amber failed";
            }
        }

        private async Task GetPrices()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await client.GetAsync(baseUrl + "/sites/" + _siteId + "/prices/current");
            var responseMessage = await response.Content.ReadAsStringAsync();
            var responseJson = JArray.Parse(responseMessage);
            foreach (var item in responseJson)
            {
                if (item["channelType"].Value<string>() == "general")
                {
                    _sellPrice = item["perKwh"].Value<decimal>();
                    _renewables = item["renewables"].Value<decimal>();
                }
                else if (item["channelType"].Value<string>() == "feedIn")
                {
                    _buyPrice = -item["perKwh"].Value<decimal>();
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
            var responseJson = JArray.Parse(responseMessage);
            foreach (var site in responseJson)
            {
                if (site["status"].Value<string>() == "active")
                {
                    _siteId = site["id"].Value<string>();
                }
            }

        }
    }
}
