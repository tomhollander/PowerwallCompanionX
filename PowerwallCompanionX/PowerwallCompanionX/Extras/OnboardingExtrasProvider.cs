using Android.Provider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PowerwallCompanionX.Extras
{
    internal class OnboardingExtrasProvider : IExtrasProvider
    {
        private static bool seen;

        public async Task<string> RefreshStatus()
        {
            if (!seen)
            {
                seen = true;
                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                {
                    return "Swipe for more 👆➡️";
                }
                else
                {
                    return "Open settings to display extra data in this position.";
                }
            }
            return null;
        }
    }
}
