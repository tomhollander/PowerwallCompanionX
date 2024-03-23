using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        }

        public static string AccessToken
        {
            get => GetProperty<string>(nameof(Properties.AccessToken), null);
            set => Application.Current.Properties[nameof(Properties.AccessToken)] = value;
        }
        public static string RefreshToken
        {
            get => GetProperty<string>(nameof(Properties.RefreshToken), null);
            set => Application.Current.Properties[nameof(Properties.RefreshToken)] = value;

        }
        public static string Email
        {
            get => GetProperty<string>(nameof(Properties.Email), null);
            set => Application.Current.Properties[nameof(Properties.Email)] = value;
        }

        public static Dictionary<string, string> AvailableSites
        {
            get
            {
                var json = GetProperty<string>(nameof(Properties.AvailableSites), null);
                return json == null ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                Application.Current.Properties[nameof(Properties.AvailableSites)] = json;
            }
        }

        public static string SiteId
        {
            get => GetProperty<string>(nameof(Properties.SiteId), null);
            set => Application.Current.Properties[nameof(Properties.SiteId)] = value;
        }

        public static bool ShowClock
        {
            get => GetProperty<bool>(nameof(Properties.ShowClock), false);
            set => Application.Current.Properties[nameof(Properties.ShowClock)] = value;
        }

        public static bool ShowGraph
        {
            get => GetProperty<bool>(nameof(Properties.ShowGraph), true);
            set => Application.Current.Properties[nameof(Properties.ShowGraph)] = value;
        }

        public static bool CyclePages
        {
            get => GetProperty<bool>(nameof(Properties.CyclePages), false);
            set => Application.Current.Properties[nameof(Properties.CyclePages)] = value;
        }

        public static bool TwoPagesOnTablet
        {
            get => GetProperty<bool>(nameof(Properties.TwoPagesOnTablet), false);
            set => Application.Current.Properties[nameof(Properties.TwoPagesOnTablet)] = value;
        }

        public static bool PlaySounds
        {
            get => GetProperty<bool>(nameof(Properties.PlaySounds), true);
            set => Application.Current.Properties[nameof(Properties.PlaySounds)] = value;
        }

        public static decimal GraphScale
        {
            get => GetProperty<decimal>(nameof(Properties.GraphScale), 1.0M);
            set => Application.Current.Properties[nameof(Properties.GraphScale)] = value;
        }

        public static double FontScale
        {
            get => GetProperty<double>(nameof(Properties.FontScale), 1);
            set => Application.Current.Properties[nameof(Properties.FontScale)] = value;
        }

        public static string SelectedExtras
        {
            get => GetProperty<string>(nameof(Properties.SelectedExtras), "None");
            set => Application.Current.Properties[nameof(Properties.SelectedExtras)] = value;
        }

        public static string WeatherCity
        {
            get => GetProperty<string>(nameof(Properties.WeatherCity), null);
            set => Application.Current.Properties[nameof(Properties.WeatherCity)] = value;
        }

        public static string WeatherUnits
        {
            get => GetProperty<string>(nameof(Properties.WeatherUnits), "C");
            set => Application.Current.Properties[nameof(Properties.WeatherUnits)] = value;
        }

        public static string AmberApiKey
        {
            get => GetProperty<string>(nameof(Properties.AmberApiKey), null);
            set => Application.Current.Properties[nameof(Properties.AmberApiKey)] = value;
        }

        public static string NewsFeedUrl
        {
            get => GetProperty<string>(nameof(Properties.NewsFeedUrl), "http://feeds.bbci.co.uk/news/world/rss.xml");
            set => Application.Current.Properties[nameof(Properties.NewsFeedUrl)] = value;
        }
        public static int WakeTeslaHours
        {
            get => GetProperty<int>(nameof(Properties.WakeTeslaHours), 24);
            set => Application.Current.Properties[nameof(Properties.WakeTeslaHours)] = value;
        }
        public static string GatewayIP
        {
            get => GetProperty<string>(nameof(Properties.GatewayIP), null);
            set => Application.Current.Properties[nameof(Properties.GatewayIP)] = value;
        }
        public static string GatewayPassword
        {
            get => GetProperty<string>(nameof(Properties.GatewayPassword), null);
            set => Application.Current.Properties[nameof(Properties.GatewayPassword)] = value;
        }

        public static bool PreventBurnIn
        {
            get => GetProperty<bool>(nameof(Properties.PreventBurnIn), false);
            set => Application.Current.Properties[nameof(Properties.PreventBurnIn)] = value;
        }

        public static bool DimAtNight
        {
            get => GetProperty<bool>(nameof(Properties.DimAtNight), false);
            set => Application.Current.Properties[nameof(Properties.DimAtNight)] = value;
        }

        private static T GetProperty<T>(string keyName, T defaultValue)
        {
            if (Application.Current.Properties.ContainsKey(keyName))
            {
                object value = Application.Current.Properties[keyName];
                try
                {
                    return (T)value;
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public static async Task SignOutUser()
        {
            Application.Current.Properties.Remove(nameof(Properties.SiteId));
            Application.Current.Properties.Remove(nameof(Properties.AvailableSites));
            Application.Current.Properties.Remove(nameof(Properties.AccessToken));
            Application.Current.Properties.Remove(nameof(Properties.RefreshToken));
            await Application.Current.SavePropertiesAsync();
        }
        public static async Task SavePropertiesAsync()
        {
            await Application.Current.SavePropertiesAsync();
        }
    }
}
