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
            SignOutCommand = new Command(OnSignOutTapped);
        }


        public Command BackCommand { get; }
        public Command SignOutCommand { get; }

        public string Email
        {
            get => Settings.Email;
        }

        public bool ShowClock
        {
            get => Settings.ShowClock;
            set => Settings.ShowClock = value;
        }

        public decimal GraphScale
        {
            get => Settings.GraphScale;
            set => Settings.GraphScale = value;
        }

        public double FontScale
        {
            get => Settings.FontScale;
            set => Settings.FontScale = value;
        }

        private async void OnBackTapped(object obj)
        {
            await Application.Current.SavePropertiesAsync();
            Application.Current.MainPage = new MainPage();
        }


        private async void OnSignOutTapped(object obj)
        {
            await Settings.SignOutUser();
            Application.Current.MainPage = new LoginPage();
        }

    }
}

