using Newtonsoft.Json.Linq;
using PowerwallCompanionX.Views;
using System.IdentityModel.Tokens.Jwt;
using TeslaAuth;

namespace PowerwallCompanionX.ViewModels
{
    class LoginViewModel 
    {
        private TeslaAuthHelper teslaAuth = new TeslaAuth.TeslaAuthHelper(TeslaAuth.TeslaAccountRegion.Unknown,
                    Keys.TeslaAppClientId, Keys.TeslaAppClientSecret, Keys.TeslaAppRedirectUrl,
                     Scopes.BuildScopeString(new[] { Scopes.EnergyDeviceData, Scopes.VechicleDeviceData}));

        public LoginViewModel()
        {
            ClearCookies();
        }

        public void ClearCookies()
        {
#if ANDROID
            Android.Webkit.CookieManager.Instance.RemoveAllCookies(null);
            Android.Webkit.CookieManager.Instance.Flush();
#endif
      
        }

        public string LoginUrl
        {
            get { return teslaAuth.GetLoginUrlForBrowser(); }
        }

        public async Task<bool> CompleteLogin(string url)
        {
            try
            {
                var tokens = await teslaAuth.GetTokenAfterLoginAsync(url);
                if (CheckTokenScopes(tokens.AccessToken))
                {
                    Settings.Email = "Tesla User";
                    Settings.AccessToken = tokens.AccessToken;
                    Settings.RefreshToken = tokens.RefreshToken;
                    await Settings.SavePropertiesAsync();

                    await GetSiteId();
                    Analytics.TrackEvent("Login success");
                    return true;
                }
                else
                {
                    Analytics.TrackEvent("Login failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("Login failed");
                Crashes.TrackError(ex);
                return false;
            }

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
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh("/api/1/products", null);
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

        private bool CheckTokenScopes(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(accessToken);
            var token = jsonToken as JwtSecurityToken;
            var scopes = token.Claims.Where(x => x.Type == "scp").Select(x => x.Value).ToList();
            return (scopes.Contains("energy_device_data"));
        }


    }
}
