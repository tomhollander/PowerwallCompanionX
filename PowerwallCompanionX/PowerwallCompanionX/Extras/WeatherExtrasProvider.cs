using Android.OS;
using Java.Net;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Android.Hardware.Lights;
using Android.Util;
using Java.Util;

namespace PowerwallCompanionX.Extras
{
    internal class WeatherExtrasProvider : IExtrasProvider
    {
        private string _apiKey;
        private string _location;
        private DateTime _lastUpdated;
        private string _lastForecast;
        private string _units;

        private Dictionary<int, string> Icons = new Dictionary<int, string>()
        {
            { 1000, "☀️" }, // Sunny
            { 1003, "🌤️" }, // Partly cloudy
            { 1006, "🌥️" }, // Cloudy
            { 1009, "☁️" }, // Overcast
            { 1030, "☁️" }, // Mist
            { 1063, "🌦️" }, // Patchy rain possible
            { 1066, "🌨️" }, // Patchy snow possible
            { 1069, "🌨️" }, // Patchy sleet possible
            { 1072, "🌦️" }, // Patchy freezing drizzle possible
            { 1087, "🌩️" }, // Thundery outbreaks possible
            { 1114, "🌨️" }, // Blowing snow
            { 1117, "🌨️" }, // Blizzard
            { 1135, "🌫️" }, // Fog
            { 1147, "🌫️" }, // Freezing fog
            { 1150, "🌦️" }, // Patchy light drizzle
            { 1153, "🌧️" }, // Light drizzle
            { 1168, "🌧️" }, // Freezing drizzle
            { 1171, "🌧️" }, // Heavy freezing drizzle
            { 1180, "🌦️" }, // Patchy light rain
            { 1183, "🌧️" }, // Light rain
            { 1186, "🌧️" }, // Moderate rain at times
            { 1189, "🌧️" }, // Moderate rain
            { 1192, "🌧️" }, // Heavy rain at times
            { 1195, "🌧️" }, // Heavy rain
            { 1198, "🌧️" }, // Light freezing rain
            { 1201, "🌧️" }, // Moderate or heavy freezing rain
            { 1204, "🌨️" }, // Light sleet
            { 1207, "🌨️" }, // Moderate or heavy sleet
            { 1210, "🌨️" }, // Patchy light snow
            { 1213, "🌨️" }, // Light snow
            { 1216, "🌨️" }, // Patchy moderate snow
            { 1219, "🌨️" }, // Moderate snow
            { 1222, "🌨️" }, // Patchy heavy snow
            { 1225, "🌨️" }, // Heavy snow
            { 1237, "🌨️" }, // Ice pellets
            { 1240, "🌧️" }, // Light rain shower
            { 1243, "🌧️" }, // Moderate or heavy rain shower
            { 1246, "🌧️" }, // Torrential rain shower
            { 1249, "🌨️" }, // Light sleet showers
            { 1252, "🌨️" }, // Moderate or heavy sleet showers
            { 1255, "🌨️" }, // Light snow showers
            { 1258, "🌨️" }, // Moderate or heavy snow showers
            { 1261, "🌨️" }, // Light showers of ice pellets
            { 1264, "🌨️" }, // Moderate or heavy showers of ice pellets
            { 1273, "🌦️" }, // Patchy light rain with thunder
            { 1276, "🌩️" }, // Moderate or heavy rain with thunder
            { 1279, "🌩️" }, // Patchy light snow with thunder
            { 1282, "🌩️" } //  Moderate or heavy snow with thunder
        };


        public WeatherExtrasProvider(string apiKey, string location, string units)
        {
            _apiKey = apiKey;
            _location = location;
            _units = units;
        }
        public async Task<string> RefreshStatus()
        {
            try
            {
                var dataAge = DateTime.Now - _lastUpdated;
                if (dataAge > TimeSpan.FromMinutes(10))
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync($"https://api.weatherapi.com/v1/forecast.json?key={_apiKey}&q={_location}&days=1&aqi=no&alerts=no");
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseMessage);
                    decimal currentTemp = _units == "C" ? responseJson["current"]["temp_c"].Value<decimal>() : responseJson["current"]["temp_f"].Value<decimal>();
                    int currentConditionsCode = responseJson["current"]["condition"]["code"].Value<int>();
                    string currentConditionsText = responseJson["current"]["condition"]["text"].Value<string>();
                    string currentIcon = currentConditionsText == "Clear" ? "🌙" : Icons[currentConditionsCode];
                    var forecastNode = responseJson["forecast"]["forecastday"][0]["day"];
                    decimal forecastTemp = _units == "C" ? forecastNode["maxtemp_c"].Value<decimal>() : forecastNode["maxtemp_f"].Value<decimal>();

                    _lastForecast = $"{currentTemp:f0}° {currentIcon} Max: {forecastTemp:f0}°";
                    _lastUpdated = DateTime.Now;
                    return _lastForecast;
                }
                else
                {
                    return _lastForecast;
                }

            }
            catch
            {
                return "Weather failed";
            }
        }

       
    }
}
