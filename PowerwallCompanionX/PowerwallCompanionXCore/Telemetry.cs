using PowerwallCompanion.Lib;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX
{
    public  static class Telemetry
    {
        private static MongoDBTelemetry mongodbTelemetry;

        static Telemetry()
        {
            mongodbTelemetry = new MongoDBTelemetry(new MauiAndroidTelemetryPlatformAdapter());
        }
        public static void TrackException(Exception ex)
        {
            // Filter out noisy web exceptions so we don't blow our Sentry quota
            if (ex is HttpRequestException || ex is WebException)
            {
                return;
            }
            mongodbTelemetry.WriteExceptionSafe(ex, true);
        }

        public static async Task TrackUnhandledException(Exception ex)
        {
            await mongodbTelemetry.WriteException(ex, false);
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> metadata)
        {
            mongodbTelemetry.WriteEventSafe(eventName, metadata);
        }

        public static void TrackEvent(string eventName)
        {
            mongodbTelemetry.WriteEventSafe(eventName, null);
        }

    }
}
