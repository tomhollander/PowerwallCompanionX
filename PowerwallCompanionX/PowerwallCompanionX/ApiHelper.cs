using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Android.Provider.ContactsContract.CommonDataKinds;

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

        public static async Task<JObject> CallPostApiWithTokenRefresh(string url)
        {
            if (Settings.AccessToken == null)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                return await CallPostApi(url);
            }
            catch (UnauthorizedAccessException)
            {
                // First fail - try refreshing
                await RefreshToken();
                return await CallPostApi(url);

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

        private static async Task<JObject> CallPostApi(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("X-Tesla-User-Agent");
            var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
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

        private static async Task RefreshToken()
        {
            try
            {
                var authHelper = new TeslaAuth.TeslaAuthHelper("PowerwallCompanion/0.0");
                var tokens = await authHelper.RefreshTokenAsync(Settings.RefreshToken);
                Settings.AccessToken = tokens.AccessToken;
                Settings.RefreshToken = tokens.RefreshToken;
                await Settings.SavePropertiesAsync();

            }
            catch
            {
                throw new UnauthorizedAccessException();
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
            else if (apiName == "PowerHistory")
            {
                return GetDemoPowerHistoryDocument(siteId);
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

        private static JObject GetDemoPowerHistoryDocument(string siteId)
        {
            return JObject.Parse(@"{
  ""response"": {
    ""serial_number"": ""xxx"",
    ""time_series"": [
      {
        ""timestamp"": ""2021-04-15T00:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3957.0547945205481,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4266.3013698630139,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4261.0958904109593,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4268.4931506849316,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4336.6438356164381,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4352.3972602739723,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4315.0684931506848,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4266.5753424657532,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4234.3150684931506,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4219.7260273972606,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4238.2758620689656,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T00:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4296.7808219178078,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4294.2465753424658,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4318.2191780821922,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:10:00+10:00"",
        ""solar_power"": 100.0765750340053,
        ""battery_power"": 4304,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:15:00+10:00"",
        ""solar_power"": 100.01277986723801,
        ""battery_power"": 4301.1034482758623,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4284.3150684931506,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 4309.0410958904113,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3371.9727891156463,
        ""grid_power"": 968.04858429577882,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4297.709405245846,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4276.8321370164012,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4280.542899040327,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4283.1245415047424,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T01:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4262.3013083622373,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4367.3564619822046,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4303.5029561291,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 4282.389177191747,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 2119.515746809032,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 259.04995969575026,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 289.29038382229737,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 288.58079510518951,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 353.93949077553947,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 355.96339552369835,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 319.71360277149773,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 275.40578515562294,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T02:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 274.89159304475129,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 276.38932638298974,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 278.84536788042851,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 328.40285248146915,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 296.02881108498087,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 261.56117090685615,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 223.23191600956329,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 220.99690288386932,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 222.07626567474784,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 286.37274708159981,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 335.58958280903022,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 354.9669811906486,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T03:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 335.89590657900459,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 275.62030841879647,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 278.44360985591493,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 275.99906900484268,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 274.78779134358444,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 278.18530414529044,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 295.92960323699532,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 292.25571828345733,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 224.13374766258343,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 220.34121408854446,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 261.28917675802154,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 279.60175895690918,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T04:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 291.86732028281853,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 354.07717448717926,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 350.07477499034309,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 310.98927548057156,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 277.239870829125,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 276.37297736128716,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 276.80167782143371,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 223.22770684385952,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 269.39798480517243,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 312.5254108823579,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1648.4864996296085,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1532.1473220929709,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T05:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1208.8523473415246,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1139.0621445491397,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1374.7252512108789,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 2857.0204453664282,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1245.2422306374328,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": -267.65517241379308,
        ""grid_power"": 1467.1076381025644,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": -1358.8356164383561,
        ""grid_power"": 2477.4043954300555,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 538.97723885105086,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 475.61125352937881,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:40:00+10:00"",
        ""solar_power"": 137.43639175206013,
        ""battery_power"": 0,
        ""grid_power"": 1387.6722017836898,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:45:00+10:00"",
        ""solar_power"": 188.87620920677708,
        ""battery_power"": 0,
        ""grid_power"": 482.59712801894096,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:50:00+10:00"",
        ""solar_power"": 226.13315101519024,
        ""battery_power"": 0,
        ""grid_power"": 552.94480151346283,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T06:55:00+10:00"",
        ""solar_power"": 303.00754714338746,
        ""battery_power"": 0,
        ""grid_power"": 571.48372900975892,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:00:00+10:00"",
        ""solar_power"": 411.248318188811,
        ""battery_power"": 0,
        ""grid_power"": 996.48148515779678,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:05:00+10:00"",
        ""solar_power"": 503.87566633553342,
        ""battery_power"": 0,
        ""grid_power"": 449.79476157879009,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:10:00+10:00"",
        ""solar_power"": 538.54275265861963,
        ""battery_power"": 0,
        ""grid_power"": 1121.1769118870006,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:15:00+10:00"",
        ""solar_power"": 547.83468059671338,
        ""battery_power"": 0,
        ""grid_power"": 1177.3793214995285,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:20:00+10:00"",
        ""solar_power"": 688.64254217278472,
        ""battery_power"": -146.64383561643837,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:25:00+10:00"",
        ""solar_power"": 805.1014955717942,
        ""battery_power"": -214.68965517241378,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:30:00+10:00"",
        ""solar_power"": 878.47841971261164,
        ""battery_power"": -249.79591836734693,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:35:00+10:00"",
        ""solar_power"": 989.27096515812286,
        ""battery_power"": -309.24657534246575,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:40:00+10:00"",
        ""solar_power"": 1080.5854416938678,
        ""battery_power"": -300.54794520547944,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:45:00+10:00"",
        ""solar_power"": 1130.7392134993045,
        ""battery_power"": -544.52054794520552,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:50:00+10:00"",
        ""solar_power"": 1174.5512645146619,
        ""battery_power"": 481.71232876712327,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T07:55:00+10:00"",
        ""solar_power"": 1251.6678550406677,
        ""battery_power"": 1310.2739726027398,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:00:00+10:00"",
        ""solar_power"": 1341.6736784634525,
        ""battery_power"": 651.57534246575347,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:05:00+10:00"",
        ""solar_power"": 1434.5272524537158,
        ""battery_power"": -780.1680672268908,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:10:00+10:00"",
        ""solar_power"": 1513.9239939722522,
        ""battery_power"": -819.79310344827582,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:15:00+10:00"",
        ""solar_power"": 1607.1258402785211,
        ""battery_power"": -895.34246575342468,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:20:00+10:00"",
        ""solar_power"": 1699.7653758769134,
        ""battery_power"": -661.97278911564626,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:25:00+10:00"",
        ""solar_power"": 1792.5028176503638,
        ""battery_power"": 908.63013698630141,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:30:00+10:00"",
        ""solar_power"": 1880.5104161092679,
        ""battery_power"": 792.7397260273973,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:35:00+10:00"",
        ""solar_power"": 1967.3030113483298,
        ""battery_power"": 664.34482758620686,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:40:00+10:00"",
        ""solar_power"": 2059.5374523344494,
        ""battery_power"": -970.54421768707482,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:45:00+10:00"",
        ""solar_power"": 2149.0799025444135,
        ""battery_power"": -1538.0821917808219,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:50:00+10:00"",
        ""solar_power"": 2238.5190630351026,
        ""battery_power"": -1452.9452054794519,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T08:55:00+10:00"",
        ""solar_power"": 2319.9945837569562,
        ""battery_power"": -1478.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:00:00+10:00"",
        ""solar_power"": 2404.3061908042596,
        ""battery_power"": -1752.2602739726028,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:05:00+10:00"",
        ""solar_power"": 2495.777204957727,
        ""battery_power"": -1842.3972602739725,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:10:00+10:00"",
        ""solar_power"": 2570.6495786724668,
        ""battery_power"": -1933.7121212121212,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:15:00+10:00"",
        ""solar_power"": 2647.3110000401325,
        ""battery_power"": -1967.3287671232877,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:20:00+10:00"",
        ""solar_power"": 2730.8291734669306,
        ""battery_power"": -2057.8767123287671,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:25:00+10:00"",
        ""solar_power"": 2826.1536831790454,
        ""battery_power"": -2067.1232876712329,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:30:00+10:00"",
        ""solar_power"": 2898.4825941111944,
        ""battery_power"": -2378.4246575342468,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:35:00+10:00"",
        ""solar_power"": 2979.3490274507703,
        ""battery_power"": -2462.6712328767121,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:40:00+10:00"",
        ""solar_power"": 3055.7378230682789,
        ""battery_power"": -2544.3835616438355,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:45:00+10:00"",
        ""solar_power"": 3129.4414483432111,
        ""battery_power"": -2600.4827586206898,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:50:00+10:00"",
        ""solar_power"": 3210.1285400390625,
        ""battery_power"": -2587.3287671232879,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T09:55:00+10:00"",
        ""solar_power"": 3275.298254561751,
        ""battery_power"": -1647.4657534246576,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:00:00+10:00"",
        ""solar_power"": 3353.0111318091822,
        ""battery_power"": -2389.4520547945203,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:05:00+10:00"",
        ""solar_power"": 3422.3264446390085,
        ""battery_power"": -2502.4827586206898,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:10:00+10:00"",
        ""solar_power"": 3498.379150390625,
        ""battery_power"": -2440,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:15:00+10:00"",
        ""solar_power"": 3558.2581870719177,
        ""battery_power"": -2472.4657534246576,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:20:00+10:00"",
        ""solar_power"": 3607.2183888056506,
        ""battery_power"": -2432.0547945205481,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:25:00+10:00"",
        ""solar_power"": 3631.5236782962329,
        ""battery_power"": -2701.9178082191779,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:30:00+10:00"",
        ""solar_power"": 3697.3501210669947,
        ""battery_power"": -2746.7808219178082,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:35:00+10:00"",
        ""solar_power"": 3762.1023116438355,
        ""battery_power"": -2823.6986301369861,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:40:00+10:00"",
        ""solar_power"": 3843.0926909348059,
        ""battery_power"": -1804.9655172413793,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:45:00+10:00"",
        ""solar_power"": 3914.1655340325342,
        ""battery_power"": -1653.7671232876712,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:50:00+10:00"",
        ""solar_power"": 3937.7972545885059,
        ""battery_power"": -1885.4109589041095,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T10:55:00+10:00"",
        ""solar_power"": 3962.0034330185144,
        ""battery_power"": -2148.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:00:00+10:00"",
        ""solar_power"": 3978.4085041202911,
        ""battery_power"": -3073.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:05:00+10:00"",
        ""solar_power"": 4000.2586493130389,
        ""battery_power"": -2964.8965517241381,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:10:00+10:00"",
        ""solar_power"": 4013.6443517348343,
        ""battery_power"": -2391.6911764705883,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:15:00+10:00"",
        ""solar_power"": 4077.8739916657746,
        ""battery_power"": -2061.027397260274,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:20:00+10:00"",
        ""solar_power"": 4133.5794694456335,
        ""battery_power"": -2389.5890410958905,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:25:00+10:00"",
        ""solar_power"": 4170.87798356681,
        ""battery_power"": -3276.5517241379312,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:30:00+10:00"",
        ""solar_power"": 4199.3947085027821,
        ""battery_power"": -3281.5753424657532,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:35:00+10:00"",
        ""solar_power"": 4206.8885648544519,
        ""battery_power"": -3143.972602739726,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:40:00+10:00"",
        ""solar_power"": 4184.1963960830481,
        ""battery_power"": -1834.7945205479452,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:45:00+10:00"",
        ""solar_power"": 4189.6966469124573,
        ""battery_power"": -1883.4931506849316,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:50:00+10:00"",
        ""solar_power"": 4224.7568225599316,
        ""battery_power"": -2309.5890410958905,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T11:55:00+10:00"",
        ""solar_power"": 4257.5113107341613,
        ""battery_power"": -3356.027397260274,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:00:00+10:00"",
        ""solar_power"": 4313.6189098619434,
        ""battery_power"": -3413.3561643835615,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:05:00+10:00"",
        ""solar_power"": 4337.4188948006467,
        ""battery_power"": -3438.4827586206898,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:10:00+10:00"",
        ""solar_power"": 4369.5821674372874,
        ""battery_power"": -3232.312925170068,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:15:00+10:00"",
        ""solar_power"": 4356.9375601990578,
        ""battery_power"": -3453.0821917808221,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:20:00+10:00"",
        ""solar_power"": 4361.8454269935346,
        ""battery_power"": -3385.5172413793102,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:25:00+10:00"",
        ""solar_power"": 4344.6099970569348,
        ""battery_power"": -3571.6438356164385,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:30:00+10:00"",
        ""solar_power"": 4351.6552868150684,
        ""battery_power"": -3567.8082191780823,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:35:00+10:00"",
        ""solar_power"": 4374.8388471211474,
        ""battery_power"": -3539.178082191781,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:40:00+10:00"",
        ""solar_power"": 4353.1611194349316,
        ""battery_power"": -3475.1369863013697,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:45:00+10:00"",
        ""solar_power"": 4341.8223599137928,
        ""battery_power"": -3440.7586206896553,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:50:00+10:00"",
        ""solar_power"": 4372.2801898276966,
        ""battery_power"": -3483.7671232876714,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T12:55:00+10:00"",
        ""solar_power"": 4379.9225606003856,
        ""battery_power"": -3553.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:00:00+10:00"",
        ""solar_power"": 4170.0731625387853,
        ""battery_power"": -3332.7659574468084,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:05:00+10:00"",
        ""solar_power"": 3305.6126741108142,
        ""battery_power"": -2480.9649122807018,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:10:00+10:00"",
        ""solar_power"": 4450.6894296151622,
        ""battery_power"": -3392.8888888888887,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:15:00+10:00"",
        ""solar_power"": 3814.4990970141266,
        ""battery_power"": -1819.3150684931506,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:20:00+10:00"",
        ""solar_power"": 4349.90185546875,
        ""battery_power"": -3398.3561643835615,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:25:00+10:00"",
        ""solar_power"": 3161.3667648841597,
        ""battery_power"": -2291.5862068965516,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:30:00+10:00"",
        ""solar_power"": 2576.4630728943707,
        ""battery_power"": -1741.986301369863,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:35:00+10:00"",
        ""solar_power"": 3924.6578966864222,
        ""battery_power"": -3094.2068965517242,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:40:00+10:00"",
        ""solar_power"": 4265.3734849903685,
        ""battery_power"": -3431.3698630136987,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:45:00+10:00"",
        ""solar_power"": 4176.1999260889343,
        ""battery_power"": -3178.2876712328766,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:50:00+10:00"",
        ""solar_power"": 4114.49180122271,
        ""battery_power"": 0,
        ""grid_power"": -3193.2055757078406,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T13:55:00+10:00"",
        ""solar_power"": 4046.3114465164813,
        ""battery_power"": 0,
        ""grid_power"": -3128.3230386498858,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:00:00+10:00"",
        ""solar_power"": 4020.1414794921875,
        ""battery_power"": 0,
        ""grid_power"": -3170.5783893637463,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:05:00+10:00"",
        ""solar_power"": 3952.5592091181506,
        ""battery_power"": 0,
        ""grid_power"": -3093.5162994175739,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:10:00+10:00"",
        ""solar_power"": 3863.2205225278253,
        ""battery_power"": 0,
        ""grid_power"": -3004.2627247849555,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:15:00+10:00"",
        ""solar_power"": 3802.8206553001928,
        ""battery_power"": 0,
        ""grid_power"": -2920.9734116123145,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:20:00+10:00"",
        ""solar_power"": 3780.5316228997217,
        ""battery_power"": 0,
        ""grid_power"": -2850.9743890631689,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:25:00+10:00"",
        ""solar_power"": 3738.0164176209332,
        ""battery_power"": 0,
        ""grid_power"": -2813.9141735965259,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:30:00+10:00"",
        ""solar_power"": 3687.7589679874786,
        ""battery_power"": 0,
        ""grid_power"": -2804.209398139013,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:35:00+10:00"",
        ""solar_power"": 3633.2608776353809,
        ""battery_power"": 0,
        ""grid_power"": -2795.6648390783021,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:40:00+10:00"",
        ""solar_power"": 3659.5343268407532,
        ""battery_power"": 0,
        ""grid_power"": -2863.2678610919274,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:45:00+10:00"",
        ""solar_power"": 3571.3824069924549,
        ""battery_power"": 0,
        ""grid_power"": -2777.7385322884338,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:50:00+10:00"",
        ""solar_power"": 3541.0135456526359,
        ""battery_power"": 0,
        ""grid_power"": -2673.8712614928786,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T14:55:00+10:00"",
        ""solar_power"": 3394.4805445177803,
        ""battery_power"": 0,
        ""grid_power"": -2534.422735911402,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:00:00+10:00"",
        ""solar_power"": 3345.146544164541,
        ""battery_power"": 0,
        ""grid_power"": -2479.1191233420859,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:05:00+10:00"",
        ""solar_power"": 3254.0544751311004,
        ""battery_power"": 0,
        ""grid_power"": -2398.6138897725982,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:10:00+10:00"",
        ""solar_power"": 3225.5554727582789,
        ""battery_power"": 0,
        ""grid_power"": -2376.6061751522234,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:15:00+10:00"",
        ""solar_power"": 3179.0894290453766,
        ""battery_power"": 0,
        ""grid_power"": -2329.4500233898425,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:20:00+10:00"",
        ""solar_power"": 2955.2926158256273,
        ""battery_power"": 0,
        ""grid_power"": -2120.5715263003394,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:25:00+10:00"",
        ""solar_power"": 2626.985752053457,
        ""battery_power"": 0,
        ""grid_power"": -1875.002796695657,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:30:00+10:00"",
        ""solar_power"": 1061.1369056179099,
        ""battery_power"": 0,
        ""grid_power"": -322.19046276562835,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:35:00+10:00"",
        ""solar_power"": 1132.8909577670163,
        ""battery_power"": 0,
        ""grid_power"": -431.83935823832473,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:40:00+10:00"",
        ""solar_power"": 1204.043727090914,
        ""battery_power"": 0,
        ""grid_power"": -524.56440922985337,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:45:00+10:00"",
        ""solar_power"": 1323.6354788166202,
        ""battery_power"": 0,
        ""grid_power"": -644.29355391410934,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:50:00+10:00"",
        ""solar_power"": 1830.1905003377835,
        ""battery_power"": 0,
        ""grid_power"": -1147.4375131162878,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T15:55:00+10:00"",
        ""solar_power"": 540.96671942349133,
        ""battery_power"": 248.34482758620689,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:00:00+10:00"",
        ""solar_power"": 1147.0918539386905,
        ""battery_power"": 0,
        ""grid_power"": -506.83258391079835,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:05:00+10:00"",
        ""solar_power"": 954.70199584960938,
        ""battery_power"": 0,
        ""grid_power"": -271.75490172921792,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:10:00+10:00"",
        ""solar_power"": 1362.5667774775256,
        ""battery_power"": 0,
        ""grid_power"": -728.69843501260834,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:15:00+10:00"",
        ""solar_power"": 1271.3538393217941,
        ""battery_power"": 0,
        ""grid_power"": -642.65895727749523,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:20:00+10:00"",
        ""solar_power"": 1714.22412109375,
        ""battery_power"": 0,
        ""grid_power"": -1073.7201365256797,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:25:00+10:00"",
        ""solar_power"": 982.66072492928345,
        ""battery_power"": 129.93103448275863,
        ""grid_power"": -392.56188664929618,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:30:00+10:00"",
        ""solar_power"": 361.15919494628906,
        ""battery_power"": 408.56164383561645,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:35:00+10:00"",
        ""solar_power"": 654.04034695559983,
        ""battery_power"": 379.58904109589042,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:40:00+10:00"",
        ""solar_power"": 479.05057347963935,
        ""battery_power"": 235.34246575342465,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:45:00+10:00"",
        ""solar_power"": 373.48571465939892,
        ""battery_power"": 460,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:50:00+10:00"",
        ""solar_power"": 378.19727699016704,
        ""battery_power"": 826.48275862068965,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T16:55:00+10:00"",
        ""solar_power"": 289.24180801600625,
        ""battery_power"": 4225.0684931506848,
        ""grid_power"": 472.60668213726723,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:00:00+10:00"",
        ""solar_power"": 277.5077153036039,
        ""battery_power"": 3568.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:05:00+10:00"",
        ""solar_power"": 221.09287408280045,
        ""battery_power"": 3529.5890410958905,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:10:00+10:00"",
        ""solar_power"": 192.68736267089844,
        ""battery_power"": 3488.478260869565,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:15:00+10:00"",
        ""solar_power"": 162.75493358743603,
        ""battery_power"": 3547.2413793103447,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:20:00+10:00"",
        ""solar_power"": 131.26319785967266,
        ""battery_power"": 3563.4246575342468,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:25:00+10:00"",
        ""solar_power"": 113.31047298483652,
        ""battery_power"": 3537.2602739726026,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2891.5068493150684,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3105.6164383561645,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3121.3698630136987,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3020.6164383561645,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3035.8620689655172,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T17:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3101.7123287671234,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3210.5517241379312,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3086.3963963963965,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3261.7808219178082,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3469.7260273972606,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3235.4109589041095,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3037.2602739726026,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2842.8082191780823,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3253.4931506849316,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3246.5753424657532,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3168.0821917808221,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3104.8630136986303,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T18:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3072.2068965517242,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3044.9315068493152,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3101.3698630136987,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3386.3503649635036,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 3357.5342465753424,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2465.7241379310344,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 834.2465753424658,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 663.013698630137,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 983.082191780822,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2225,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2099.6575342465753,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2159.3835616438355,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T19:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 2011.7123287671234,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 1555.3424657534247,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 927.65517241379314,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 1006.7808219178082,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 1057.5172413793102,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 1027.4657534246576,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 990.68493150684935,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 818.21917808219177,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 674.2465753424658,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 629.17808219178085,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 638.15068493150682,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 710.54794520547944,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T20:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 668.35616438356169,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 592.8767123287671,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 539.04109589041093,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 512.71428571428567,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 483.013698630137,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 501.43835616438355,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 553.63013698630141,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 513.69863013698625,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 1022.1917808219179,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 872.67123287671234,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 461.30136986301369,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 301.09589041095893,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T21:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 344.13793103448273,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 368.90410958904107,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 354.17808219178085,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 295.41095890410958,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 293.58620689655174,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 490.27397260273972,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 560.61643835616439,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 643.35616438356169,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 641.31034482758616,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 396.66666666666669,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 295.34246575342468,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 312.32876712328766,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T22:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 305.20547945205482,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 323.28767123287673,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 376.890756302521,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 384.52554744525548,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 325.58620689655174,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 302.38095238095241,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 299.0344827586207,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 300.47945205479454,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 339.65753424657532,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 367.26027397260276,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 367.26027397260276,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 293.86206896551727,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-15T23:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 294.89795918367349,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 296.48275862068965,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 296.36986301369865,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 347.31034482758622,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 376.23287671232879,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 367.00680272108843,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 309.24137931034483,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 301.50684931506851,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 300.958904109589,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 301.43835616438355,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 359.45205479452056,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 369.0344827586207,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T00:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 345.958904109589,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 299.10958904109589,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 298.90410958904107,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 302.16417910447763,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 254.62068965517241,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 306.64383561643837,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 325.10344827586209,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 297.53424657534248,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 249.31506849315068,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 308.15068493150687,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 306.57534246575341,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 301.78082191780823,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T01:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 376.027397260274,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 385.93103448275861,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 349.31972789115645,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 224.14460862258386,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 280.32192815493232,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 278.25601123130485,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 279.23277220007492,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 334.60999164842582,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 351.83961249051026,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 292.58349395124878,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 229.11838191829315,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 223.70870250545136,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T02:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 225.95382319411186,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 225.39823534735319,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 343.05733651984229,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 357.51607633616828,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 331.12215355464389,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 286.67580690775833,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 283.709550073702,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 281.22895831277924,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 279.36141411245688,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 321.72637709526168,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 350.38937558213325,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 334.13609342705712,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T03:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 278.03922694718756,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 270.20103887830464,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 223.09832432839724,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 223.96604409936356,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 257.17477521504441,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 297.03576082726045,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 308.02482565518085,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 291.80613191160438,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 286.57811870313668,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 288.38819498558564,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 288.89542825254676,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 295.14890072443711,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T04:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 350.84731547037762,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 348.2566042669888,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 286.22790712853,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 277.814218468862,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 277.18776925404865,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 268.95587095063308,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 224.46738553209369,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 289.06779252666314,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 296.74851616168843,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 255.54003077990387,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 221.98997467511322,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:50:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 282.39375102683289,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T05:55:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 506.02818543943641,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:00:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 412.38945587367226,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:05:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 379.0367728455426,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:10:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 853.45977556952118,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:15:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1307.1852057208753,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:20:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1234.2239072094226,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:25:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1201.7565890534283,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:30:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1129.9427824934869,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:35:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 1070.4251860004581,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:40:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 996.55708704909239,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:45:00+10:00"",
        ""solar_power"": 0,
        ""battery_power"": 0,
        ""grid_power"": 520.14712574057387,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:50:00+10:00"",
        ""solar_power"": 157.42536445513164,
        ""battery_power"": 0,
        ""grid_power"": 667.18794260939512,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T06:55:00+10:00"",
        ""solar_power"": 207.08434044824887,
        ""battery_power"": 0,
        ""grid_power"": 2653.8914367728039,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:00:00+10:00"",
        ""solar_power"": 376.73959998561912,
        ""battery_power"": 0,
        ""grid_power"": 693.61542956878066,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:05:00+10:00"",
        ""solar_power"": 471.19722089375534,
        ""battery_power"": 0,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:10:00+10:00"",
        ""solar_power"": 437.03477854271455,
        ""battery_power"": 0,
        ""grid_power"": 126.49904722873478,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:15:00+10:00"",
        ""solar_power"": 256.85323349972987,
        ""battery_power"": 0,
        ""grid_power"": 388.98049664328283,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:20:00+10:00"",
        ""solar_power"": 512.9219770039598,
        ""battery_power"": 0,
        ""grid_power"": 229.51808510863617,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:25:00+10:00"",
        ""solar_power"": 868.21763004668776,
        ""battery_power"": -227.67123287671234,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:30:00+10:00"",
        ""solar_power"": 695.83116003585189,
        ""battery_power"": -192.53424657534248,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:35:00+10:00"",
        ""solar_power"": 1026.1952405955694,
        ""battery_power"": -384.72602739726028,
        ""grid_power"": 331.82411771277862,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:40:00+10:00"",
        ""solar_power"": 1140.490446960034,
        ""battery_power"": -721.36054421768711,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:45:00+10:00"",
        ""solar_power"": 1281.7157445447199,
        ""battery_power"": -868.13793103448279,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:50:00+10:00"",
        ""solar_power"": 901.5906290550754,
        ""battery_power"": -461.50684931506851,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T07:55:00+10:00"",
        ""solar_power"": 1019.9331723565924,
        ""battery_power"": -505.82191780821915,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:00:00+10:00"",
        ""solar_power"": 1464.344919701145,
        ""battery_power"": -912.39726027397262,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:05:00+10:00"",
        ""solar_power"": 1736.4710868939962,
        ""battery_power"": -1194.1780821917807,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:10:00+10:00"",
        ""solar_power"": 1540.7169114204303,
        ""battery_power"": -1069.9315068493152,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:15:00+10:00"",
        ""solar_power"": 1780.8213651474207,
        ""battery_power"": -1358.9041095890411,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:20:00+10:00"",
        ""solar_power"": 2014.4683661099139,
        ""battery_power"": -1628.2758620689656,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:25:00+10:00"",
        ""solar_power"": 1897.116863093964,
        ""battery_power"": -1522.4657534246576,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:30:00+10:00"",
        ""solar_power"": 1125.3466558587061,
        ""battery_power"": -765,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:35:00+10:00"",
        ""solar_power"": 908.46218830265411,
        ""battery_power"": -472.46575342465752,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:40:00+10:00"",
        ""solar_power"": 1518.286160403735,
        ""battery_power"": -1033.6986301369864,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:45:00+10:00"",
        ""solar_power"": 1180.2426707646619,
        ""battery_power"": -738.69863013698625,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:50:00+10:00"",
        ""solar_power"": 930.7636292340004,
        ""battery_power"": -520.82191780821915,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T08:55:00+10:00"",
        ""solar_power"": 863.946826171875,
        ""battery_power"": -455.24137931034483,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:00:00+10:00"",
        ""solar_power"": 829.99663239592439,
        ""battery_power"": -423.28671328671328,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:05:00+10:00"",
        ""solar_power"": 1170.7972466605049,
        ""battery_power"": -759.19642857142856,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:10:00+10:00"",
        ""solar_power"": 669.13195842586151,
        ""battery_power"": -211.986301369863,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:15:00+10:00"",
        ""solar_power"": 2060.7833514178242,
        ""battery_power"": -1567.4814814814815,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:20:00+10:00"",
        ""solar_power"": 2961.7988231084119,
        ""battery_power"": -2497.0547945205481,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:25:00+10:00"",
        ""solar_power"": 2398.7847160443866,
        ""battery_power"": -2023.5616438356165,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:30:00+10:00"",
        ""solar_power"": 2364.827446088399,
        ""battery_power"": -1988.013698630137,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:35:00+10:00"",
        ""solar_power"": 1574.2228192891159,
        ""battery_power"": -1204.4520547945206,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      },
      {
        ""timestamp"": ""2021-04-16T09:40:00+10:00"",
        ""solar_power"": 1087.6358072916667,
        ""battery_power"": -728,
        ""grid_power"": 0,
        ""grid_services_power"": 0,
        ""generator_power"": 0
      }
    ]
  }
}
");
        }

      
    }


}
