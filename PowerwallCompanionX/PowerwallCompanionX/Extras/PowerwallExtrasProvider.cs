using PowerwallCompanionX.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX.Extras
{
    internal class PowerwallExtrasProvider : IExtrasProvider
    {
        private MainViewModel _viewModel;
        public PowerwallExtrasProvider(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public async Task<string> RefreshStatus()
        {
            return $"🔋Total capacity: {_viewModel.TotalPackEnergy/1000:f2}kWh";
        }
    }
}
