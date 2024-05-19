using PowerwallCompanionX.ViewModels;

namespace PowerwallCompanionX.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        private LoginViewModel viewModel = new LoginViewModel();
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = viewModel;
            Analytics.TrackEvent("LoginPage opened");
        }

        protected override void OnAppearing()
        {
            webView.IsVisible = true;
            waitMessage.IsVisible = false;
            base.OnAppearing();
        }

        private async void webView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.Contains(Keys.TeslaAppRedirectUrl))
            {
                warningBanner.IsVisible = false;
                errorBanner.IsVisible = false;
                webView.IsVisible = false;
                waitMessage.IsVisible = true;
                if (await viewModel.CompleteLogin(e.Url))
                {
                    Application.Current.MainPage = new MainPage();
                }
                else
                {
                    viewModel.ClearCookies();
                    errorBanner.IsVisible = true;
                    waitMessage.IsVisible = false;
                    webView.IsVisible = true;
                    webView.Source = viewModel.LoginUrl;
                }

            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            viewModel.LoginAsDemoUser();
        }
        
        private void LearnMoreHyperlink_Tapped(object sender, EventArgs e)
        {
            moreInfo.IsVisible = true;
            webView.IsVisible = false;
        }

        private async void TeslaAccountHyperlink_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://accounts.tesla.com/account-settings/security?tab=tpty-apps"); 
        }

        private void hideAuthInfoButton_Clicked(object sender, EventArgs e)
        {
            moreInfo.IsVisible = false;
            webView.IsVisible = true;
        }
    }
}