using kitchenview.Models;
using Microsoft.Extensions.Configuration;
using Splat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenMeteo.Weather.Forecast.ResponseModel;
using System.Linq;

namespace kitchenview.DataAccess
{
    public class WeatherDataAccess : IEnableLogger, IDataAccess<Weather>
    {
        private readonly IConfiguration _configuration;

        private readonly HttpClient _client;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public WeatherDataAccess(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<IEnumerable<Weather>?> GetData()
        {
            _tokenSource.Cancel();
            try
            {
                var latitude = _configuration?.GetValue<string>("Controls:Weather:Location:Latitude");
                var longitude = _configuration?.GetValue<string>("Controls:Weather:Location:Longitude");
                var weatherEndpoint = "https://api.open-meteo.com/v1/forecast?";
                weatherEndpoint += $"latitude={latitude}&longitude={longitude}";
                weatherEndpoint += "&timezone=Europe%2FBerlin&forecast_days=3&models=icon_seamless";
                weatherEndpoint += "&hourly=temperature_2m,surface_pressure,windspeed_10m,winddirection_10m,weathercode,precipitation,precipitation_probability";

                if (weatherEndpoint is null)
                {
                    this.Log().Error("Invalid Weather Endpoint. Cannot load Weather!");
                    return null;
                }

                var response = await _client.GetAsync(weatherEndpoint).ConfigureAwait(false);
                if (response?.StatusCode != HttpStatusCode.OK)
                {
                    return null!;
                }
                else
                {
                    var content = await response?.Content?.ReadAsStringAsync();
                    if (content is null)
                        return null;

                    var meteoData = JsonConvert.DeserializeObject<WeatherForecast>(content);
                    if (meteoData is null)
                        return null;

                    return await ConvertEventsToAppointments(meteoData) ?? [];
                }

            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Error while parsing weather response into internal weather model");
                return null;
            }
        }

        internal async Task<IEnumerable<Weather>> ConvertEventsToAppointments(WeatherForecast meteoData)
        {
            this.Log().Debug("Processing new data");
            var currentDay = DateTime.Now;
            var weatherData = new Weather()
            {
                Today = new WeatherDay(),
                Tomorrow = new WeatherDay(),
                DayAfterTomorrow = new WeatherDay(),
            };

            try
            {
                weatherData.Latitude = meteoData.Latitude;
                weatherData.Longitude = meteoData.Longitude;
                weatherData.Elevation = meteoData.Elevation;
                weatherData.Today.Temperature = meteoData.Hourly.Temperature_2m[currentDay.Hour] ?? -273.15f;
                weatherData.Today.Pressure = meteoData.Hourly.Surface_pressure[currentDay.Hour] ?? 0;
                weatherData.Today.Windspeed = meteoData.Hourly.Windspeed_10m[currentDay.Hour] ?? -1;
                weatherData.Today.WindDirection = meteoData.Hourly.Winddirection_10m[currentDay.Hour] ?? -1;
                weatherData.Today.WeatherCode = (WeatherCode)(meteoData.Hourly.Weathercode[currentDay.Hour] ?? -1);
                weatherData.Today.PrecipitationProbability = meteoData.Hourly.Precipitation_probability[currentDay.Hour] ?? -1;
                weatherData.Today.Precipitation = meteoData.Hourly.Precipitation[currentDay.Hour] ?? -1;

                var temperatureChunks = meteoData.Hourly.Temperature_2m.Chunk(24);
                var pressureChunks = meteoData.Hourly.Surface_pressure.Chunk(24);
                var windSpeedChunks = meteoData.Hourly.Windspeed_10m.Chunk(24);
                var windDirectionChunks = meteoData.Hourly.Winddirection_10m.Chunk(24);
                var weatherCodeChunks = meteoData.Hourly.Weathercode.Chunk(24);
                var precipitationProbabilityChunks = meteoData.Hourly.Precipitation_probability.Chunk(24);
                var precipitationChunks = meteoData.Hourly.Precipitation.Chunk(24);
                weatherData.Tomorrow.Temperature = temperatureChunks.ElementAt(1).Average() ?? -273.15f;
                weatherData.Tomorrow.Pressure = pressureChunks.ElementAt(1).Average() ?? 0;
                weatherData.Tomorrow.Windspeed = windSpeedChunks.ElementAt(1).Average() ?? -1;
                weatherData.Tomorrow.WindDirection = Convert.ToInt32(windDirectionChunks.ElementAt(1).Average() ?? -1);
                weatherData.Tomorrow.WeatherCode = (WeatherCode)(meteoData.Hourly.Weathercode[currentDay.Hour + 24] ?? -1);
                weatherData.Tomorrow.PrecipitationProbability = Convert.ToInt32(precipitationProbabilityChunks.ElementAt(1).Average() ?? -1);
                weatherData.Tomorrow.Precipitation = precipitationChunks.ElementAt(1).Average() ?? -273.15f;;

                weatherData.DayAfterTomorrow.Temperature = temperatureChunks.ElementAt(2).Average() ?? -273.15f;
                weatherData.DayAfterTomorrow.Pressure = pressureChunks.ElementAt(2).Average() ?? 0;
                weatherData.DayAfterTomorrow.Windspeed = windSpeedChunks.ElementAt(2).Average() ?? -1;
                weatherData.DayAfterTomorrow.WindDirection = Convert.ToInt32(windDirectionChunks.ElementAt(2).Average() ?? -1);
                weatherData.DayAfterTomorrow.WeatherCode = (WeatherCode)(meteoData.Hourly.Weathercode[currentDay.Hour + 48] ?? -1);
                weatherData.DayAfterTomorrow.PrecipitationProbability = Convert.ToInt32(precipitationProbabilityChunks.ElementAt(2).Average() ?? -1);
                weatherData.DayAfterTomorrow.Precipitation = precipitationChunks.ElementAt(2).Average() ?? -273.15f;;

                return new List<Weather>()
                {
                    weatherData
                };
            }
            catch (Exception exp)
            {
                this.Log().Error(exp, "Could not parse weather api response into weather data");
                return null!;
            }
        }
    }
}