using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX
{
    internal static class Crashes
    {
        public static void TrackError(Exception ex)
        {
            // Filter out noisy web exceptions so we don't blow our Sentry quota
            if (ex is HttpRequestException || ex is WebException)
            {
                return;
            }
            SentrySdk.CaptureException(ex);
        }
    }
}
