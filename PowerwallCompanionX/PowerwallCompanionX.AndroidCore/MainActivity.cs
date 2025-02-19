﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Devices;
using Microsoft.Maui;

namespace PowerwallCompanionX.Droid
{
    [Activity(MainLauncher = true, Label = "Powerwall Companion", Icon = "@mipmap/myIcon", Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : Microsoft.Maui.MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);

            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Microsoft.Maui.Devices.DeviceDisplay.KeepScreenOn = true;
        }

    }
}