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


        public static async Task<JObject> CallGetApiWithTokenRefresh(string url, string demoApiName)
        {
            if (Settings.AccessToken == null)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                return await CallGetApi(url, demoApiName);
            }
            catch (UnauthorizedAccessException)
            {
                // First fail - try refreshing
                await RefreshToken();
                return await CallGetApi(url, demoApiName);

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

        private static async Task<JObject> CallGetApi(string url, string demoApiName)
        {
            if (Settings.AccessToken.StartsWith("DEMO"))
            {
                return GetDemoDocument(demoApiName, Settings.SiteId);
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

        private static JObject GetDemoDocument(string apiName, string siteId)
        {
            if (apiName == "LiveStatus")
            {
                return GetDemoPowerDocument(siteId);
            }
            else if (apiName == "EnergyHistory")
            {
                return GetDemoEnergyDocument(siteId);
            }
            throw new InvalidOperationException();
        }

        private static JObject GetDemoEnergyDocument(string siteId)
        {
            return JObject.Parse(@"{  
   ""response"":{  

""serial_number"":""1118431-01-F--T17G0004126"",
""period"":""day"",
""time_series"":[

{
                ""timestamp"":""2018-02-19T00:00:00+11:00"",
            ""solar_energy_exported"":22673.4666666966,
            ""grid_energy_imported"":926.377223184099,
            ""grid_energy_exported_from_solar"":2132.24583429517,
            ""grid_energy_exported_from_battery"":0,
            ""battery_energy_exported"":9760,
            ""battery_energy_imported_from_grid"":0,
            ""battery_energy_imported_from_solar"":13670,
            ""consumer_energy_imported_from_grid"":926.377223184099,
            ""consumer_energy_imported_from_solar"":6871.22083240142,
            ""consumer_energy_imported_from_battery"":9760

},
         {
                ""timestamp"":""2018-02-20T00:00:00+11:00"",
            ""solar_energy_exported"":14308.0916666668,
            ""grid_energy_imported"":96.1911119243596,
            ""grid_energy_exported_from_solar"":132.668889702298,
            ""grid_energy_exported_from_battery"":0,
            ""battery_energy_exported"":3810,
            ""battery_energy_imported_from_grid"":0,
            ""battery_energy_imported_from_solar"":9410,
            ""consumer_energy_imported_from_grid"":96.1911119243596,
            ""consumer_energy_imported_from_solar"":4765.42277696449,
            ""consumer_energy_imported_from_battery"":3810

}
      ],
      ""self_consumption"":{
                ""timestamp"":""2018-02-18T15:33:00+11:00"",
         ""solar"":37.5705682339493,
         ""battery"":57.4940374042163

}
        }
    }");
        }

        private static JObject GetDemoPowerDocument(string siteId)
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
