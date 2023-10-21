using Newtonsoft.Json.Linq;
using PowerwallCompanionX.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeslaAuth;
using Xamarin.Forms;

namespace PowerwallCompanionX.ViewModels
{
    class LoginViewModel 
    {
        private TeslaAuthHelper teslaAuth = new TeslaAuthHelper();

        public LoginViewModel()
        {
            DependencyService.Get<IClearCookies>().Clear();
        }

        public string LoginUrl
        {
            get { return teslaAuth.GetLoginUrlForBrowser(); }
        }

        public async Task CompleteLogin(string url)
        {
            var tokens = await teslaAuth.GetTokenAfterLoginAsync(url);
            Settings.Email = "Tesla User";
            Settings.AccessToken = tokens.AccessToken;
            Settings.RefreshToken = tokens.RefreshToken;
            await Settings.SavePropertiesAsync();

            await GetSiteId();

            Application.Current.MainPage = new MainPage();
        }

        public async Task LoginAsDemoUser()
        {
            Settings.Email = "demo@example.com";
            Settings.AccessToken = "DEMO";
            Settings.RefreshToken = "DEMO";
            await Settings.SavePropertiesAsync();
            await GetSiteId();

            Application.Current.MainPage = new MainPage();

        }

 

        private async Task GetSiteId()
        {
            if (Settings.AccessToken == "DEMO")
            {
                Settings.SiteId = "DEMO1";
                Settings.AvailableSites = new Dictionary<string, string>
                {
                    { "DEMO1", "Demo Powerwall 1" },
                    { "DEMO2", "Demo Powerwall 2"}
                };
                await Settings.SavePropertiesAsync();
                return;
            }
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            var availableSites = new Dictionary<string, string>();
            bool foundSite = false;
            foreach (var product in productsResponse["response"])
            {
                if (product["resource_type"]?.Value<string>() == "battery" && product["energy_site_id"] != null)
                {
                    var siteName = product["site_name"].Value<string>();
                    var id = product["energy_site_id"].Value<long>();
                    if (!foundSite)
                    {
                        Settings.SiteId = id.ToString();
                        foundSite = true;
                    }
                    availableSites.Add(id.ToString(), siteName);
                    
                }
            }
            if (foundSite)
            {
                Settings.AvailableSites = availableSites;
                await Settings.SavePropertiesAsync();
            }
            else
            {
                throw new Exception("Powerwall site not found");
            }
            
        }


    }
}
