using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using PanCardView;
using Syncfusion.Maui.Core.Hosting;
using System;

namespace PowerwallCompanionX.Droid
{
    [Application]
    public class MainApplication : MauiApplication
{
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Telemetry.TrackUnhandledException((Exception)e.ExceptionObject);
            };
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            builder.ConfigureSyncfusionCore();
            builder.UseCardsView();

            builder
              .UseMauiApp<App>();

            Telemetry.TrackUser();
            Telemetry.TrackEvent("SessionStart", Settings.ToDictionary());
            return builder.Build();
        }
    }
    //protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
}
