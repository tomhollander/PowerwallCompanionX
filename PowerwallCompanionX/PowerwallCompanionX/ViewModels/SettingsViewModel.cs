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
            get => (string) Application.Current.Properties[AppProperties.Email];
        }

        public bool ShowClock
        {
            get
            {
                if (Application.Current.Properties.ContainsKey(AppProperties.ShowClock))
                {
                    return (bool)Application.Current.Properties[AppProperties.ShowClock];
                }
                return false;
            }
            set => Application.Current.Properties[AppProperties.ShowClock] = value;
        }

        public decimal GraphScale
        {
            get
            {
                if (Application.Current.Properties.ContainsKey(AppProperties.GraphScale))
                {
                    return (decimal)Application.Current.Properties[AppProperties.GraphScale];
                }
                return 1.0M;
            }
            set => Application.Current.Properties[AppProperties.GraphScale] = value;
        }

        private async void OnBackTapped(object obj)
        {
            await Application.Current.SavePropertiesAsync();
            Application.Current.MainPage = new MainPage();
        }


        private async void OnSignOutTapped(object obj)
        {
            Application.Current.Properties.Remove(AppProperties.Email);
            Application.Current.Properties.Remove(AppProperties.SiteId);
            Application.Current.Properties.Remove(AppProperties.AccessToken);
            Application.Current.Properties.Remove(AppProperties.RefreshToken);
            await Application.Current.SavePropertiesAsync();
            Application.Current.MainPage = new LoginPage();
        }

    }
}

