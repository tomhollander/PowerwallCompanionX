using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
        }

        protected override void OnAppearing()
        {
            webView.IsVisible = true;
            base.OnAppearing();
        }

        private void webView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.Contains("void/callback"))
            {
                webView.IsVisible = false;
                viewModel.CompleteLogin(e.Url);

            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            viewModel.LoginAsDemoUser();
        }
    }
}