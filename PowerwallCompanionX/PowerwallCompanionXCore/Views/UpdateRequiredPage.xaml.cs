using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;

namespace PowerwallCompanionX.Views
{
    public partial class UpdateRequiredPage : ContentPage
    {
        public UpdateRequiredPage()
        {
            InitializeComponent();
        }

        private async void UpdateButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync("market://details?id=" + AppInfo.PackageName);
            }
            catch
            {
                await Launcher.OpenAsync("https://play.google.com/store/apps/details?id=" + AppInfo.PackageName);
            }
        }
    }
}