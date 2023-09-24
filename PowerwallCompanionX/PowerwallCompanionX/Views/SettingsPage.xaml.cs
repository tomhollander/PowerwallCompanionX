using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerwallCompanionX.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel viewModel;
        public SettingsPage()
        {
            InitializeComponent();
            viewModel = new SettingsViewModel();
            this.BindingContext = viewModel;
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
            {
                cyclePages.IsEnabled = false;
            }
        }
    }
}