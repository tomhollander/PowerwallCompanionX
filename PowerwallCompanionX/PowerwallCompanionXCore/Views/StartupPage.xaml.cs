using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using PowerwallCompanionX;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

namespace PowerwallCompanionX.Views
{
    public partial class StartupPage : ContentPage
    {
        public StartupPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (await IsVersionObsolete())
            {
                Application.Current.Windows[0].Page = new UpdateRequiredPage();
            }
            else
            {
                if (IsLoggedIn())
                {
                    Application.Current.Windows[0].Page = new MainPage();
                }
                else
                {
                    Application.Current.Windows[0].Page = new LoginPage();
                }
            }
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(Settings.SiteId) && !String.IsNullOrEmpty(Settings.AccessToken) && GetTokenAzp(Settings.AccessToken) == Keys.TeslaAppClientId;
        }

        private async Task<bool> IsVersionObsolete()
        {
            // Compare the app version against the minimum version stored in the cloud
            try
            {
                using (var client = new HttpClient())
                {
                    var minVersionString = await client.GetStringAsync("https://tomsapps2.blob.core.windows.net/powerwall-companion/android-minimum-version.txt");
                    if (Version.TryParse(minVersionString?.Trim(), out var minVersion))
                    {
                        return AppInfo.Version < minVersion;
                    }
                }

            }
            catch
            {
                // Ignore errors
            }
            return false;
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