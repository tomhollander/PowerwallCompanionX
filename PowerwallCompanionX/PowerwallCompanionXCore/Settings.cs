using System.Text.Json;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.Communication;

namespace PowerwallCompanionX
{
    static class Settings
    {
        private enum Properties
        {
            AccessToken,
            RefreshToken,
            Email,
            SiteId,
            AvailableSites,
            ShowClock,
            GraphScale,
            FontScale,
            CyclePages,
            TwoPagesOnTablet,
            ShowGraph,
            PlaySounds,
            SelectedExtras,
            WeatherApiKey,
            WeatherCity,
            WeatherUnits,
            AmberApiKey,
            NewsFeedUrl,
            WakeTeslaHours,
            PreventBurnIn,
            DimAtNight,
            GatewayIP,
            GatewayPassword,
            ShowEnergyCosts,
            InstallationTimeZone,
            ShowSiteName,
            DailySupplyCharge,
        }

        public static string AccessToken
        {
            get => GetProperty<string>(nameof(Properties.AccessToken), null);
            set => Preferences.Default.Set(nameof(Properties.AccessToken), value); 
        }
        public static string RefreshToken
        {
            get => GetProperty<string>(nameof(Properties.RefreshToken), null);
            set => Preferences.Default.Set(nameof(Properties.RefreshToken), value);

        }
        public static string Email
        {
            get => GetProperty<string>(nameof(Properties.Email), null);
            set => Preferences.Default.Set(nameof(Properties.Email), value);
        }

        public static Dictionary<string, string> AvailableSites
        {
            get
            {
                var json = GetProperty<string>(nameof(Properties.AvailableSites), null);
                return json == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            set
            {
                var json = JsonSerializer.Serialize(value);
                Preferences.Default.Set(nameof(Properties.AvailableSites), json);
            }
        }

        public static string SiteId
        {
            get => GetProperty<string>(nameof(Properties.SiteId), null);
            set => Preferences.Default.Set(nameof(Properties.SiteId), value);
        }

        public static bool ShowClock
        {
            get => GetProperty<bool>(nameof(Properties.ShowClock), false);
            set => Preferences.Default.Set(nameof(Properties.ShowClock), value);
        }

        public static bool ShowGraph
        {
            get => GetProperty<bool>(nameof(Properties.ShowGraph), true);
            set => Preferences.Default.Set(nameof(Properties.ShowGraph), value);
        }

        public static bool CyclePages
        {
            get => GetProperty<bool>(nameof(Properties.CyclePages), false);
            set => Preferences.Default.Set(nameof(Properties.CyclePages), value);
        }

        public static bool TwoPagesOnTablet
        {
            get => GetProperty<bool>(nameof(Properties.TwoPagesOnTablet), false);
            set => Preferences.Default.Set(nameof(Properties.TwoPagesOnTablet), value);
        }

        public static bool PlaySounds
        {
            get => GetProperty<bool>(nameof(Properties.PlaySounds), true);
            set => Preferences.Default.Set(nameof(Properties.PlaySounds), value);
        }

        public static double GraphScale
        {
            get => GetProperty<double>(nameof(Properties.GraphScale), 1.0);
            set => Preferences.Default.Set(nameof(Properties.GraphScale), value);
        }

        public static double FontScale
        {
            get => GetProperty<double>(nameof(Properties.FontScale), 1);
            set => Preferences.Default.Set(nameof(Properties.FontScale), value);
        }

        public static string SelectedExtras
        {
            get => GetProperty<string>(nameof(Properties.SelectedExtras), "None");
            set => Preferences.Default.Set(nameof(Properties.SelectedExtras), value);
        }

        public static string WeatherCity
        {
            get => GetProperty<string>(nameof(Properties.WeatherCity), null);
            set => Preferences.Default.Set(nameof(Properties.WeatherCity), value);
        }

        public static string WeatherUnits
        {
            get => GetProperty<string>(nameof(Properties.WeatherUnits), "C");
            set => Preferences.Default.Set(nameof(Properties.WeatherUnits), value);
        }

        public static string AmberApiKey
        {
            get => GetProperty<string>(nameof(Properties.AmberApiKey), null);
            set => Preferences.Default.Set(nameof(Properties.AmberApiKey), value);
        }

        public static string NewsFeedUrl
        {
            get => GetProperty<string>(nameof(Properties.NewsFeedUrl), "http://feeds.bbci.co.uk/news/world/rss.xml");
            set => Preferences.Default.Set(nameof(Properties.NewsFeedUrl), value);
        }
        public static int WakeTeslaHours
        {
            get => GetProperty<int>(nameof(Properties.WakeTeslaHours), 24);
            set => Preferences.Default.Set(nameof(Properties.WakeTeslaHours), value);
        }
        public static string GatewayIP
        {
            get => GetProperty<string>(nameof(Properties.GatewayIP), null);
            set => Preferences.Default.Set(nameof(Properties.GatewayIP), value);
        }
        public static string GatewayPassword
        {
            get => GetProperty<string>(nameof(Properties.GatewayPassword), null);
            set => Preferences.Default.Set(nameof(Properties.GatewayPassword), value);
        }

        public static string InstallationTimeZone
        {
            get => GetProperty<string>(nameof(Properties.InstallationTimeZone), null);
            set => Preferences.Default.Set(nameof(Properties.InstallationTimeZone), value);
        }

        public static bool PreventBurnIn
        {
            get => GetProperty<bool>(nameof(Properties.PreventBurnIn), false);
            set => Preferences.Default.Set(nameof(Properties.PreventBurnIn), value);
        }

        public static bool DimAtNight
        {
            get => GetProperty<bool>(nameof(Properties.DimAtNight), false);
            set => Preferences.Default.Set(nameof(Properties.DimAtNight), value);
        }

        public static bool ShowEnergyCosts
        {
            get => GetProperty<bool>(nameof(Properties.ShowEnergyCosts), false);
            set => Preferences.Default.Set(nameof(Properties.ShowEnergyCosts), value);
        }

        public static decimal DailySupplyCharge
        {
            get  
            {
                var value = GetProperty<string>(nameof(Properties.DailySupplyCharge), "0.0");
                return decimal.TryParse(value, out decimal result) ? result : 0.0M;
            }
            set => Preferences.Default.Set(nameof(Properties.DailySupplyCharge), value.ToString());
        }

        public static bool ShowSiteName
        {
            get => GetProperty<bool>(nameof(Properties.ShowSiteName), false);
            set => Preferences.Default.Set(nameof(Properties.ShowSiteName), value);
        }


        private static T GetProperty<T>(string keyName, T defaultValue)
        {
            try
            {
                return Preferences.Default.Get(keyName, defaultValue);
            }
            catch
            {
                // It shouldn't throw, but it does sometimes. 
                return defaultValue;
            }
            
        }

        public static void SignOutUser()
        {
            Preferences.Default.Remove(nameof(Properties.SiteId));
            Preferences.Default.Remove(nameof(Properties.AvailableSites));
            Preferences.Default.Remove(nameof(Properties.AccessToken));
            Preferences.Default.Remove(nameof(Properties.RefreshToken));
            Preferences.Default.Remove(nameof(Properties.InstallationTimeZone));
            //await Application.Current.SavePropertiesAsync();
        }

    }
}
