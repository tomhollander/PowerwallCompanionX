﻿using PowerwallCompanionX.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace PowerwallCompanionX.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
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

        public bool ShowGraph
        {
            get => Settings.ShowGraph;
            set => Settings.ShowGraph = value;
        }

        public bool CyclePages
        {
            get => Settings.CyclePages;
            set => Settings.CyclePages = value;
        }

        public bool PlaySounds
        {
            get => Settings.PlaySounds;
            set => Settings.PlaySounds = value;
        }

        public decimal GraphScale
        {
            get => Settings.GraphScale;
            set { 
                Settings.GraphScale = value;
                NotifyPropertyChanged(nameof(GraphScale));
            }
        }

        public double FontScale
        {
            get => Settings.FontScale;
            set => Settings.FontScale = value;
        }

        public List<KeyValuePair<string, string>> AvailableSites
        {
            get
            {
                var availableSites = Settings.AvailableSites;
                if (availableSites == null) // Older versions may sign in without this list
                {
                    return new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(Settings.SiteId, "Default")
                    };
                }
                else
                {
                    return availableSites.ToList();
                }
            }
        }

        public KeyValuePair<string, string> SelectedSite
        {
            get => AvailableSites.Where(s => s.Key == Settings.SiteId).FirstOrDefault();
            set => Settings.SiteId = value.Key;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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

