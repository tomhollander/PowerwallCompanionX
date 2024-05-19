using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.View;

namespace PowerwallCompanionX.Droid
{
    [Activity(Label = "Powerwall Companion", Icon = "@mipmap/myIcon", Theme = "@style/MainTheme", 
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

            //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
            //{
            //    Window.SetDecorFitsSystemWindows(false);
            //    var windowInsetsController = Window.DecorView.WindowInsetsController;
            //    if (windowInsetsController != null)
            //    {
            //        windowInsetsController.Hide(WindowInsetsCompat.Type.NavigationBars());
            //    }
            //}
            //else
            //{
            //    var uiOptions = SystemUiFlags.HideNavigation;
            //    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            //}

            // LoadApplication(new App());
        }

    }
}