using Newtonsoft.Json.Linq;
using PowerwallCompanionX.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PowerwallCompanionX.ViewModels
{
    class LoginViewModel : INotifyPropertyChanged
    {
        private string email;
        private string password;
        private string mfaCode;
        private string errorMessage;
        private bool isBusy;

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged("Email");
            }
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public string MfaCode
        {
            get => mfaCode;
            set => mfaCode = value;
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
            
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }

        }

        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            Email = Settings.Email;
        }

        private async void OnLoginClicked(object obj)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = "";

                if (email == "demo@example.com" && password == "demo")
                {
                    Settings.Email = email;
                    Settings.AccessToken = "DEMO";
                    Settings.RefreshToken = "DEMO";
                }
                else
                {
                    var auth = new TeslaAuth.TeslaAuthHelper("PowerwallCompanion/0.0");
                    var tokens = await auth.AuthenticateAsync(email, password, mfaCode);

                    Settings.Email = email;
                    Settings.AccessToken = tokens.AccessToken;
                    Settings.RefreshToken = tokens.RefreshToken;
                }
                await Settings.SavePropertiesAsync();

                await GetSiteId();
                IsBusy = false;
                Application.Current.MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GetSiteId()
        {
            if (Settings.AccessToken == "DEMO")
            {
                Settings.SiteId = "DEMO1";
                Settings.AvailableSites = new Dictionary<string, string>
                {
                    { "DEMO1", "Demo Powerwall 1" },
                    { "DEMO2", "Demo Powerwall 2"}
                };
                await Settings.SavePropertiesAsync();
                return;
            }
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", null);
            var availableSites = new Dictionary<string, string>();
            bool foundSite = false;
            foreach (var product in productsResponse["response"])
            {
                if (product["resource_type"]?.Value<string>() == "battery" && product["energy_site_id"] != null)
                {
                    var siteName = product["site_name"].Value<string>();
                    var id = product["energy_site_id"].Value<long>();
                    if (!foundSite)
                    {
                        Settings.SiteId = id.ToString();
                        foundSite = true;
                    }
                    availableSites.Add(id.ToString(), siteName);
                    
                }
            }
            if (foundSite)
            {
                Settings.AvailableSites = availableSites;
                await Settings.SavePropertiesAsync();
            }
            else
            {
                throw new Exception("Powerwall site not found");
            }
            
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
