using Xamarin.Forms;
using System.Net;
using Android.Webkit;
using PowerwallCompanionX;

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