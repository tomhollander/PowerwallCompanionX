﻿using PowerwallCompanion.Lib;
using PowerwallCompanionX.Views;
using System.IdentityModel.Tokens.Jwt;
using TeslaAuth;

namespace PowerwallCompanionX.ViewModels
{
    class LoginViewModel 
    {
        private TeslaAuthHelper teslaAuth = new TeslaAuth.TeslaAuthHelper(TeslaAuth.TeslaAccountRegion.Unknown,
                    Keys.TeslaAppClientId, Keys.TeslaAppClientSecret, Keys.TeslaAppRedirectUrl,
                     Scopes.BuildScopeString(new[] { Scopes.EnergyDeviceData, Scopes.VehicleDeviceData}));

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
                var powerwallApi = new PowerwallApi(null, new MauiPlatformAdapter());
                var tokens = await teslaAuth.GetTokenAfterLoginAsync(url);
                if (CheckTokenScopes(tokens.AccessToken))
                {
                    Settings.Email = "Tesla User";
                    Settings.AccessToken = tokens.AccessToken;
                    Settings.RefreshToken = tokens.RefreshToken;
                    Settings.SiteId = await powerwallApi.GetFirstSiteId();
                    Settings.AvailableSites = await powerwallApi.GetEnergySites();
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
            Settings.SiteId = "DEMO";

            Application.Current.MainPage = new MainPage();

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
