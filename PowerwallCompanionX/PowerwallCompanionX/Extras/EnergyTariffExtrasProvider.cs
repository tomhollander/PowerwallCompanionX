using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static Android.App.Assist.AssistStructure;

namespace PowerwallCompanionX.Extras
{
    public class EnergyTariffExtrasProvider : IExtrasProvider
    {
        JObject ratePlan;
        TariffHelper tariffHelper;

        public EnergyTariffExtrasProvider()
        {
            Analytics.TrackEvent("EnergyTariffExtrasProvider initialised");
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

                string message = "";
                switch (tariff.DisplayName)
                {
                    case "On Peak":
                        message = "🔴 ";
                        break;
                    case "Partial Peak":
                        message = "🟠 ";
                        break;
                    case "Off Peak":
                        message = "🟢 ";
                        break;
                    case "Super Off Peak":
                        message = "🔵 ";
                        break;
                    default:
                        break;
                }    

                message += tariff.DisplayName;
                message += $": {FormatCurrency(prices.Item1)}";
                if (prices.Item2 > 0)
                {
                    message += $" (Feed in: {FormatCurrency(prices.Item2)})";
                }
                return message;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return "Rates unavailable";
            }
      
        }

        public string FormatCurrency(decimal rate)
        {
       
            var currencySymbol = NumberFormatInfo.CurrentInfo.CurrencySymbol;

            if (rate == 0)
            {
                return String.Empty;
            }
            else if (rate > 1)
            {
                return rate.ToString("C");
            }
            else if (currencySymbol == "$" || currencySymbol == "€")
            {
                return (rate * 100).ToString("#.#") + "c";
            }
            else if (currencySymbol == "£")
            {
                return (rate * 100).ToString("#.#") + "p";
            }
            else
            {
                return rate.ToString("C");
            }

        }
    }
}
