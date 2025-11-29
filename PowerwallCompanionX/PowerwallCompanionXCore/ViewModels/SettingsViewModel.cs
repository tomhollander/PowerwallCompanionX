using PowerwallCompanionX.Views;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel.Communication;

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

        public string PowerDisplayMode
        {
            get => Settings.PowerDisplayMode;
            set => Settings.PowerDisplayMode = value;
        }

        public bool ShowClock
        {
            get => Settings.ShowClock;
            set => Settings.ShowClock = value;
        }

        public bool ShowAnimations
        {
            get => Settings.ShowAnimations;
            set => Settings.ShowAnimations = value;
        }
      

        public bool ShowSiteName
        {
            get => Settings.ShowSiteName;
            set => Settings.ShowSiteName = value;
        }

        public bool ShowGraph
        {
            get => Settings.ShowGraph;
            set => Settings.ShowGraph = value;
        }

        public bool TwoPagesOnTablet
        {
            get => Settings.TwoPagesOnTablet;
            set => Settings.TwoPagesOnTablet = value;
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

        public bool PreventBurnIn
        {
            get => Settings.PreventBurnIn;
            set => Settings.PreventBurnIn = value;
        }

        public bool DimAtNight
        {
            get => Settings.DimAtNight;
            set => Settings.DimAtNight = value;
        }

        public bool ShowEnergyCosts
        {
            get => Settings.ShowEnergyCosts;
            set => Settings.ShowEnergyCosts = value;
        }

        public int PowerDecimals
        {
            get => Settings.PowerDecimals;
            set
            {
                Settings.PowerDecimals = value;
                NotifyPropertyChanged(nameof(PowerDecimals));
            }
        }

        public int EnergyDecimals
        {
            get => Settings.EnergyDecimals;
            set
            {
                Settings.EnergyDecimals = value;
                NotifyPropertyChanged(nameof(EnergyDecimals));
            }
        }

        public List<KeyValuePair<string, string>> AvailablePowerDisplayModes
        {
            get => new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("Graph", "Graph"),
                new KeyValuePair<string, string>("Flow", "Flow"),
            };
        }

        public KeyValuePair<string, string> SelectedBatteryHealthMode
        {
            get => AvailableBatteryHealthModes.Where(s => s.Key == Settings.BatteryHealthMode).FirstOrDefault();
            set => Settings.BatteryHealthMode = value.Key;
        }

        public List<KeyValuePair<string, string>> AvailableBatteryHealthModes
        {
            get => new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("Estimates", "Estimates"),
                new KeyValuePair<string, string>("Gateway", "Gateway"),
            };
        }

        public KeyValuePair<string, string> SelectedPowerDisplayMode
        {
            get => AvailablePowerDisplayModes.Where(s => s.Key == Settings.PowerDisplayMode).FirstOrDefault();
            set => Settings.PowerDisplayMode = value.Key;
        }

        public List<KeyValuePair<string, string>> AvailableExtras
        {
            get => new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("None", "None"),
                new KeyValuePair<string, string>("Tariffs", "Energy rates (Tesla rate plan)"),
                new KeyValuePair<string, string>("Amber", "Amber electric prices"),
                new KeyValuePair<string, string>("Powerwall", "Powerwall battery health"),
                new KeyValuePair<string, string>("News", "RSS News"),
                new KeyValuePair<string, string>("Weather", "Weather"),
            };
        }

        public KeyValuePair<string, string> SelectedExtras
        {
            get => AvailableExtras.Where(e => e.Key == Settings.SelectedExtras).FirstOrDefault();
            set => Settings.SelectedExtras = value.Key;
        }


        public string WeatherCity
        {
            get => Settings.WeatherCity;
            set => Settings.WeatherCity = value;
        }

        public string WeatherUnits
        {
            get => Settings.WeatherUnits;
            set => Settings.WeatherUnits = value;
        }

        public string AmberApiKey
        {
            get => Settings.AmberApiKey;
            set => Settings.AmberApiKey = value;
        }

        public string NewsFeedUrl
        {
            get => Settings.NewsFeedUrl;
            set => Settings.NewsFeedUrl = value;
        }

        public int WakeTeslaHours
        {
            get => Settings.WakeTeslaHours;
            set => Settings.WakeTeslaHours = value;
        }

        public string GatewayIP
        {
            get => Settings.GatewayIP;
            set => Settings.GatewayIP = value;
        }

        public string GatewayPassword
        {
            get => Settings.GatewayPassword;
            set => Settings.GatewayPassword = value;
        }

        public double GraphScale
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
        
        public string DailySupplyCharge
        {
            get => Settings.DailySupplyCharge.ToString("0.00");
            set
            {
                if (decimal.TryParse(value, out decimal result))
                {
                    Settings.DailySupplyCharge = result;
                }
                else
                {
                    Settings.DailySupplyCharge = 0;
                }
            }
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

        public string AppVersion
        {
            get
            {
                return VersionTracking.CurrentVersion;
            }
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
            //await Application.Current.SavePropertiesAsync();
            Application.Current.Windows[0].Page = new MainPage();
        }


        private async void OnSignOutTapped(object obj)
        {
            Settings.SignOutUser();
            Application.Current.Windows[0].Page = new LoginPage();
        }


    }
}

