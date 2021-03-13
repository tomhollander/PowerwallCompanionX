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
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDExMTYzQDMxMzgyZTM0MmUzME9wUTNNVDVoMW5ZdEtoR0dqL2JNOGlIcVFEUDJuelVzQUJ0VDgyc0NIS0k9");
            InitializeComponent();

            if (Current.Properties.ContainsKey(AppProperties.SiteId) &&
                !string.IsNullOrEmpty((string)Current.Properties[AppProperties.SiteId]))
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
