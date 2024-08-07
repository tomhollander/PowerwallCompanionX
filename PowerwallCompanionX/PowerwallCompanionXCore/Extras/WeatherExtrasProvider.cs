﻿using System.Text.Json.Nodes;

namespace PowerwallCompanionX.Extras
{
    internal class WeatherExtrasProvider : IExtrasProvider
    {
        private string _apiKey;
        private string _location;
        private DateTime _lastUpdated;
        private string _lastCurrent;
        private string _lastForecast;
        private string _units;
        private int lastPage = 0;

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


        public WeatherExtrasProvider(string location, string units)
        {
            Telemetry.TrackEvent("WeatherExtrasProvider initialised");
            _apiKey = Keys.WeatherApi;
            _location = location;
            _units = units;
        }
        public async Task<string> RefreshStatus()
        {
            try
            {
                var dataAge = DateTime.Now - _lastUpdated;
                if (dataAge > TimeSpan.FromMinutes(15))
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync($"https://api.weatherapi.com/v1/forecast.json?key={_apiKey}&q={_location}&days=3&aqi=no&alerts=no");
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    var responseJson = (JsonObject) JsonObject.Parse(responseMessage);
                    _lastCurrent = GetCurrentConditions(responseJson);
                    _lastForecast = GetForecast(responseJson);
                    _lastUpdated = DateTime.Now;
                    
                }

                if (++lastPage % 2 == 0)
                {
                    lastPage = 0;
                    return _lastCurrent;
                }
                else
                {
                    return _lastForecast;
                }
    
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                return "Weather failed";
            }
        }

        private string GetCurrentConditions(JsonObject responseJson)
        {
            if (responseJson["location"] == null)
            {
                return "Weather: location not found";
            }
            string location = responseJson["location"]["name"].GetValue<string>();
            decimal currentTemp = _units == "C" ? responseJson["current"]["temp_c"].GetValue<decimal>() : responseJson["current"]["temp_f"].GetValue<decimal>();
            int currentConditionsCode = responseJson["current"]["condition"]["code"].GetValue<int>();
            string currentConditionsText = responseJson["current"]["condition"]["text"].GetValue<string>();
            string currentIcon = currentConditionsText == "Clear" ? "🌙" : Icons[currentConditionsCode];
            var forecastNode = responseJson["forecast"]["forecastday"][0]["day"];
            decimal forecastTemp = _units == "C" ? forecastNode["maxtemp_c"].GetValue<decimal>() : forecastNode["maxtemp_f"].GetValue<decimal>();

            return $"{location}: {currentTemp:f0}°{currentIcon}";
        }

        private string GetForecast(JsonObject responseJson)
        {
            if (responseJson["forecast"] == null)
            {
                return "Weather: location not found";
            }

            string result = String.Empty;
            foreach (var dayNode in responseJson["forecast"]["forecastday"].AsArray())
            {
                DateTime date = DateTime.Parse(dayNode["date"].GetValue<string>());
                decimal maxTemp = _units == "C" ? dayNode["day"]["maxtemp_c"].GetValue<decimal>() : dayNode["day"]["maxtemp_f"].GetValue<decimal>();
                int conditionsCode = dayNode["day"]["condition"]["code"].GetValue<int>();
                string conditionsIcon = Icons[conditionsCode];
                result += $"{date:ddd}: {maxTemp:f0}°{conditionsIcon} ";
            }

            return result;
        }
    }
}
