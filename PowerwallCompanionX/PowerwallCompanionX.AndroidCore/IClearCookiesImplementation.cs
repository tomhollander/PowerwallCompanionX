using System.Net;
using Android.Webkit;
using PowerwallCompanionX;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

[assembly: Dependency(typeof(PowerwallCompanionX.Droid.IClearCookiesImplementation))]
namespace PowerwallCompanionX.Droid
{
    public class IClearCookiesImplementation : IClearCookies
    {
        public void Clear()
        {
            var cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookie();
        }
    }
}