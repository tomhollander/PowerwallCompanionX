using Newtonsoft.Json.Linq;
using PowerwallCompanionX;
using PowerwallCompanionX.Converters;
using PowerwallCompanionX.ViewModels;
using System.Globalization;

namespace PowerwallCompanionX.Extras
{
    public class EnergyTariffExtrasProvider : IExtrasProvider
    {
        JObject ratePlan;
        TariffHelper tariffHelper;
        MainViewModel mainViewModel;

        public EnergyTariffExtrasProvider(MainViewModel mainViewModel)
        {
            Analytics.TrackEvent("EnergyTariffExtrasProvider initialised");
            this.mainViewModel = mainViewModel;
        }
        public async Task<string> RefreshStatus()
        {
            try
            {
                if (ratePlan == null)
                {
                    ratePlan = await ApiHelper.CallGetApiWithTokenRefresh($"/api/1/energy_sites/{Settings.SiteId}/tariff_rate", "TariffRate");
                }
                if (tariffHelper == null)
                {
                    tariffHelper = new TariffHelper(ratePlan);
                }
                var tariff = tariffHelper.GetTariffForInstant(DateTime.Now);
                var prices = tariffHelper.GetRatesForTariff(tariff);
                var converter = new RateCurrencyConverter();

                string message = "";
                string tariffIcon = "";
                const string green = "🟢";
                const string red = "🔴";
                const string yellow = "🟡";
                const string blue = "🔵";
                const string sun = "☀️";

                if (!tariffHelper.IsSingleRatePlan)
                {
                    switch (tariff.DisplayName)
                    {
                        case "On Peak":
                            tariffIcon = red;
                            break;
                        case "Partial Peak":
                            tariffIcon = yellow;
                            break;
                        case "Off Peak":
                            tariffIcon = green;
                            break;
                        case "Super Off Peak":
                            tariffIcon = blue;
                            break;
                        default:
                            break;
                    }
                }

                if (mainViewModel.HomeFromGrid > 50)
                {
                    var cost = prices.Item1 * (decimal)(mainViewModel.HomeFromGrid / 1000); 
                    message += $"{FormatCurrency(cost)}/h ∙ {tariffIcon}{FormatCurrency(prices.Item1)}/kWh";
                }
                else if (mainViewModel.SolarToGrid > 50)
                {
                    var feedIn = prices.Item2 * (decimal)(mainViewModel.SolarToGrid / 1000);
                    message += $"Feed in: {FormatCurrency(feedIn)}/h ∙ {sun}{FormatCurrency(prices.Item2)}/kWh";
                }
                else if (!tariffHelper.IsSingleRatePlan)
                {
                    message += $"{tariff.DisplayName} {tariffIcon}{FormatCurrency(prices.Item1)}/kWh";
                }

                return message;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return "Rates unavailable";
            }
      
        }

        private string FormatCurrency(decimal value)
        {
            var converter = new RateCurrencyConverter();
            return (string)converter.Convert(value, typeof(string), null, CultureInfo.CurrentCulture);
        }

    }
}
