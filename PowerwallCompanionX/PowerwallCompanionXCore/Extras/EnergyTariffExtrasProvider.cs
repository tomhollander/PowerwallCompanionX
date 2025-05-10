using PowerwallCompanion.Lib;
using PowerwallCompanionX.Converters;
using PowerwallCompanionX.ViewModels;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PowerwallCompanionX.Extras
{
    public class EnergyTariffExtrasProvider : IExtrasProvider
    {
        JsonObject ratePlan;
        ITariffProvider tariffHelper;
        MainViewModel mainViewModel;
        PowerwallApi powerwallApi;

        public EnergyTariffExtrasProvider(MainViewModel mainViewModel)
        {
            Telemetry.TrackEvent("EnergyTariffExtrasProvider initialised");
            this.mainViewModel = mainViewModel;
            this.powerwallApi = new PowerwallApi(Settings.SiteId, new MauiPlatformAdapter());

        }
        public async Task<string> RefreshStatus()
        {
            try
            {
                if (ratePlan == null)
                {
                    ratePlan = await powerwallApi.GetRatePlan();
                }
                if (tariffHelper == null)
                {
                    tariffHelper = new TeslaRatePlanTariffProvider(ratePlan, Settings.DailySupplyCharge);
                }
                var tariff = await tariffHelper.GetInstantaneousTariff();
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

                if (mainViewModel.InstantaneousPower.HomeFromGrid > 50)
                {
                    var cost = prices.Item1 * (decimal)(mainViewModel.InstantaneousPower.HomeFromGrid / 1000); 
                    message += $"{FormatCurrency(cost)}/h ∙ {tariffIcon}{FormatCurrency(prices.Item1)}/kWh";
                }
                else if (mainViewModel.InstantaneousPower.SolarToGrid > 50)
                {
                    var feedIn = prices.Item2 * (decimal)(mainViewModel.InstantaneousPower.SolarToGrid / 1000);
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
                Telemetry.TrackException(ex);
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
