using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX
{
    internal static class Crashes
    {
        public static void TrackError(Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }
}
