using PowerwallCompanion.Lib;
using System.Net;

namespace PowerwallCompanionX
{
    public  static class Telemetry
    {
        private static AzureFunctionsTelemetry telemetry;

        static Telemetry()
        {
            telemetry = new AzureFunctionsTelemetry(new MauiAndroidTelemetryPlatformAdapter());
        }
        public static void TrackException(Exception ex)
        {
            // Filter out noisy web exceptions so we don't blow our Sentry quota
            if (ex is HttpRequestException || ex is WebException)
            {
                return;
            }
            telemetry.WriteExceptionSafe(ex, true);
        }

        public static async Task TrackUnhandledException(Exception ex)
        {
            await telemetry.WriteException(ex, false);
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> metadata)
        {
            telemetry.WriteEventSafe(eventName, metadata);
        }

        public static void TrackEvent(string eventName)
        {
            telemetry.WriteEventSafe(eventName, null);
        }

    }
}
