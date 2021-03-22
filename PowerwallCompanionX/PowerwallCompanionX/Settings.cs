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
            FontScale
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
