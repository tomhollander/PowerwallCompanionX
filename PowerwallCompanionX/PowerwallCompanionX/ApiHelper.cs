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

        private static string AccessToken
        {
            get
            {
                return Application.Current.Properties[AppProperties.AccessToken].ToString();
            }
        }

        public static async Task<JObject> CallGetApiWithTokenRefresh(string url, string demoId)
        {
            if (AccessToken == null)
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

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
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


        private static async Task RefreshToken()
        {
            try
            {
                var authHelper = new TeslaAuth.TeslaAuthHelper("PowerwallCompanion/0.0");
                var token = await authHelper.RefreshTokenAsync(Application.Current.Properties[AppProperties.RefreshToken].ToString(), TeslaAuth.TeslaAccountRegion.Unknown);
                Application.Current.Properties[AppProperties.AccessToken] = token;
               
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }
        }
    }


}
