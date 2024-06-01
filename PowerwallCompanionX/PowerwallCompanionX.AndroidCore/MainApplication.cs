using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using PanCardView;
using Sentry;
using Syncfusion.Maui.Core.Hosting;

namespace PowerwallCompanionX.Droid
{
    [Application]
    public class MainApplication : MauiApplication
{
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = new ApplicationException("An unhandled exception occurred", (Exception)e.ExceptionObject);
                SentrySdk.CaptureException(ex);
            };
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            builder.ConfigureSyncfusionCore();
            builder.UseCardsView();

            builder
              .UseMauiApp<App>()

              // Add this section anywhere on the builder:
              .UseSentry(options =>
              {
                  // The DSN is the only required setting.
                  options.Dsn = "https://a48cbf8d91dca55d794d8a06aac78c43@o4507321171902464.ingest.us.sentry.io/4507321174589440";

                  options.Debug = true;

                  // Other Sentry options can be set here.
              });
            return builder.Build();
        }
    }
    //protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
}
