using PowerwallCompanionX.Views;
using System;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.IdentityModel.Tokens.Jwt;

namespace PowerwallCompanionX
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Keys.SyncFusion);
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            if (!string.IsNullOrEmpty(Settings.SiteId) && !String.IsNullOrEmpty(Settings.AccessToken) && GetTokenAzp(Settings.AccessToken) == Keys.TeslaAppClientId)
            {
                return new Window(new MainPage());
            }
            else
            {
                return new Window(new LoginPage());
            }
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private static string GetTokenAzp(string accessToken)
        {
            try
            {
                // Parse JWT and get azp claim
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(accessToken);
                var azp = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "azp").Value;
                return azp;
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                return null;
            }
        }

    }
}
