using PowerwallCompanionX.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerwallCompanionX
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Keys.SyncFusion);
            InitializeComponent();

            if (!string.IsNullOrEmpty(Settings.SiteId))
            {
                MainPage = new MainPage();
            }
            else
            {
                MainPage = new LoginPage();
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
