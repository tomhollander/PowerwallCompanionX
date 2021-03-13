using PowerwallCompanionX.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.ViewModels
{
    class SettingsViewModel
    {
        public SettingsViewModel()
        {
            BackCommand = new Command(OnBackTapped);
        }

        public Command BackCommand { get; }

        private void OnBackTapped(object obj)
        {
            Application.Current.MainPage = new MainPage();
        }
    }
}
