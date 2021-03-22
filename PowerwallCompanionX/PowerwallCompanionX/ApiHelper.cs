using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PowerwallCompanionX
{
    static class ApiHelper
    {
        public const string BaseUrl = "https://owner-api.teslamotors.com";
        public static object lockObj = new object();


        public static async Task<JObject> CallGetApiWithTokenRefresh(string url, string demoId)
        {
            if (Settings.AccessToken == null)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                return await CallGetApi(url, demoId);
            }
            catch (UnauthorizedAccessException)
            {
                // First fail - try refreshing
                await RefreshToken();
                return await CallGetApi(url, demoId);

            }
        }

        public static async Task<JObject> CallApiIgnoreCerts(string url)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (var client = new HttpClient(httpClientHandler))
                {
                    var response = await client.GetAsync(new Uri(url));
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return JObject.Parse(responseMessage);
                    }
                    else
                    {
                        throw new HttpRequestException(responseMessage);
                    }
                }
            }
        }

        private static async Task<JObject> CallGetApi(string url, string demoId)
        {
            if (Settings.AccessToken.StartsWith("DEMO"))
            {
                return await GetDemoDocument(Settings.SiteId);
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("X-Tesla-User-Agent");
            var response = await client.GetAsync(url);
            var responseMessage = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JObject.Parse(responseMessage);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                throw new HttpRequestException(responseMessage);
            }
        }

        private static async Task<JObject> GetDemoDocument(string siteId)
        {
            if (siteId == "DEMO2")
            {
                return JObject.Parse(@"{
    ""response"": {
    ""solar_power"": 1373.6399993896484,
    ""energy_left"": 13000,
    ""total_pack_energy"": 14057,
    ""battery_power"": 1626,
    ""load_power"": 3000,
    ""grid_status"": ""Inactive"",
    ""grid_power"": 0,
    ""timestamp"": ""2018-02-28T07:12:32+11:00""
            }}");
            }
            else
            {
                return JObject.Parse(@"{
    ""response"": {
    ""solar_power"": 3473.6399993896484,
    ""energy_left"": 6552.78947368421,
    ""total_pack_energy"": 14057,
    ""battery_power"": -1900,
    ""load_power"": 1583.669994354248,
    ""grid_status"": ""Active"",
    ""grid_power"": -19.9700050354004,
    ""timestamp"": ""2018-02-28T07:12:32+11:00""
            }}");
            }

        }

        private static async Task RefreshToken()
        {
            try
            {
                var authHelper = new TeslaAuth.TeslaAuthHelper("PowerwallCompanion / 0.0");
                var tokens = await authHelper.RefreshTokenAsync(Settings.RefreshToken, TeslaAuth.TeslaAccountRegion.Unknown);
                Settings.AccessToken = tokens.AccessToken;
                Settings.RefreshToken = tokens.RefreshToken;
                await Settings.SavePropertiesAsync();

            }
            catch
            {
                throw new UnauthorizedAccessException();
            }
        }
    }


}
