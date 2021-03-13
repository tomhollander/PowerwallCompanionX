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
            if (Application.Current.Properties.ContainsKey(AppProperties.Email))
            {
                Email = (string)Application.Current.Properties[AppProperties.Email];
            }
        }

        private async void OnLoginClicked(object obj)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = "";
                var auth = new TeslaAuth.TeslaAuthHelper("PowerwallCompanion/0.0");
                var tokens = await auth.AuthenticateAsync(email, password, mfaCode);

                Application.Current.Properties[AppProperties.Email] = email;
                Application.Current.Properties[AppProperties.AccessToken] = tokens.AccessToken;
                Application.Current.Properties[AppProperties.RefreshToken] = tokens.RefreshToken;
                await Application.Current.SavePropertiesAsync();

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
            var productsResponse = await ApiHelper.CallGetApiWithTokenRefresh(ApiHelper.BaseUrl + "/api/1/products", "Products");
            foreach (var product in productsResponse["response"])
            {
                if (product["resource_type"]?.Value<string>() == "battery" && product["energy_site_id"] != null)
                {
                    var id = product["energy_site_id"].Value<long>();
                    Application.Current.Properties[AppProperties.SiteId] = id.ToString();
                    await Application.Current.SavePropertiesAsync();
                    return;
                }
            }
            throw new Exception("Powerwall site not found");
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
