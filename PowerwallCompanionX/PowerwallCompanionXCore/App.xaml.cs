using PowerwallCompanionX.Views;
using System;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PowerwallCompanionX
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Keys.SyncFusion);
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            if (!string.IsNullOrEmpty(Settings.SiteId))
            {
                return new Window(new MainPage());
            }
            else
            {
                return new Window(new LoginPage());
            }
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}
