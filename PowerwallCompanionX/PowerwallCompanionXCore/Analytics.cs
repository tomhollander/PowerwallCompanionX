using Sentry;

namespace PowerwallCompanionX
{
    internal static class Analytics
    {
        public static void TrackEvent(string eventName)
        {
            SentrySdk.CaptureMessage(eventName);
        }
    }
}
