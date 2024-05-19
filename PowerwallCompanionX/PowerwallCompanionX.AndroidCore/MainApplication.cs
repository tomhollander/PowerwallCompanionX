using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace PowerwallCompanionX.Droid
{
    [Application]
    public class MainApplication : MauiApplication
{
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            builder.ConfigureSyncfusionCore();
            return builder.Build();
        }
    }
    //protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
}
