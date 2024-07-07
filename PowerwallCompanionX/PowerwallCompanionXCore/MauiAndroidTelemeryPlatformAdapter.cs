using PowerwallCompanion.Lib;
using System.Globalization;
using System.Text;

namespace PowerwallCompanionX
{
    public class MauiAndroidTelemeryPlatformAdapter : ITelemetryPlatformAdapter
    {
        public string Platform => "Android";
        public string UserId
        {
            get
            {
#if ANDROID
                var context = global::Android.App.Application.Context;
                return global::Android.Provider.Settings.Secure.GetString(global::Android.App.Application.Context.ContentResolver, global::Android.Provider.Settings.Secure.AndroidId);
#else
                return null;
#endif
            }
        }

        public string AppName => Microsoft.Maui.ApplicationModel.AppInfo.Current.PackageName;

        public string AppVersion => Microsoft.Maui.ApplicationModel.AppInfo.Current.VersionString;

        public string OSVersion => DeviceInfo.Current.VersionString;

        public string Region => CultureInfo.CurrentCulture.Name;
    }
}
