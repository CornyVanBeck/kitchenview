using kitchenview.DataAccess;
using kitchenview.Models;
using kitchenview.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace kitchenview.Controls.ViewModels
{
    public class AwesomeWeatherViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IConfiguration _configuration;

        private readonly IDataAccess<Weather> _weatherData;

        private readonly Timer _weatherInterval;

        public string? LocationLabel
        {
            get;
            set;
        }

        #region WeatherFields
        public string? CurrentTemperature
        {
            get;
            set;
        }

        public string? CurrentPressure
        {
            get;
            set;
        }

        public string? CurrentWindspeed
        {
            get;
            set;
        }

        public string? CurrentWindDirection
        {
            get;
            set;
        }

        public string? CurrentWeatherCode
        {
            get;
            set;
        }

        public string? CurrentPrecipitationProbability
        {
            get;
            set;
        }

        public string? CurrentWeatherCodeGlyph
        {
            get;
            set;
        }

        public string? TomorrowTemperature
        {
            get;
            set;
        }

        public string? TomorrowPressure
        {
            get;
            set;
        }

        public string? TomorrowWindspeed
        {
            get;
            set;
        }

        public string? TomorrowWindDirection
        {
            get;
            set;
        }

        public string? TomorrowWeatherCode
        {
            get;
            set;
        }

        public string? TomorrowPrecipitationProbability
        {
            get;
            set;
        }

        public string? TomorrowWeatherCodeGlyph
        {
            get;
            set;
        }

        public string? DayAfterTomorrowTemperature
        {
            get;
            set;
        }

        public string? DayAfterTomorrowPressure
        {
            get;
            set;
        }

        public string? DayAfterTomorrowWindspeed
        {
            get;
            set;
        }

        public string? DayAfterTomorrowWindDirection
        {
            get;
            set;
        }

        public string? DayAfterTomorrowWeatherCode
        {
            get;
            set;
        }

        public string? DayAfterTomorrowPrecipitationProbability
        {
            get;
            set;
        }

        public string? DayAfterTomorrowWeatherCodeGlyph
        {
            get;
            set;
        }
        #endregion

        public string? LastUpdated
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AwesomeWeatherViewModel(IConfiguration configuration, IDataAccess<Weather> dataAccess)
        {
            _weatherData = dataAccess;

            _weatherInterval = new Timer();
            _weatherInterval.Interval = TimeSpan.Parse(configuration["Controls:Weather:UpdateInterval"]).TotalMilliseconds;
            _weatherInterval.Elapsed += OnUpdateWeather;
            _weatherInterval.Start();

            LocationLabel = configuration.GetValue<string?>("Controls:Weather:Location:Label");

            CurrentWeatherCodeGlyph = "";
            TomorrowWeatherCodeGlyph = "";
            DayAfterTomorrowWeatherCodeGlyph = "";

            LoadWeatherData();
            LastUpdated = $"Zuletzt aktualisiert: {DateTime.Now:HH:mm}";
        }

        internal void OnUpdateWeather(object? sender, EventArgs? args)
        {
            Debug.WriteLine("Updating weather");
            LoadWeatherData();
            LastUpdated = $"Zuletzt aktualisiert: {DateTime.Now:HH:mm}";
            OnPropertyChanged(nameof(LastUpdated));
        }

        internal void LoadWeatherData()
        {
            var parsedWeatherData = _weatherData?.GetData().Result;
            if (parsedWeatherData is null)
                return;

            var weatherData = parsedWeatherData.ElementAt(0);

            LocationLabel += $" ({weatherData.Elevation} m.ü.M.)";

            CurrentTemperature = Math.Round(weatherData.Today.Temperature, 2) + " °C";
            TomorrowTemperature = Math.Round(weatherData.Tomorrow.Temperature, 2) + " °C";
            DayAfterTomorrowTemperature = Math.Round(weatherData.DayAfterTomorrow.Temperature, 2) + " °C";
            OnPropertyChanged(nameof(CurrentTemperature));
            OnPropertyChanged(nameof(TomorrowTemperature));
            OnPropertyChanged(nameof(DayAfterTomorrowTemperature));

            CurrentPressure = Math.Round(weatherData.Today.Pressure, 2) + " hPa";
            TomorrowPressure = Math.Round(weatherData.Tomorrow.Pressure, 2) + " hPa";
            DayAfterTomorrowPressure = Math.Round(weatherData.DayAfterTomorrow.Pressure, 2) + " hPa";
            OnPropertyChanged(nameof(CurrentPressure));
            OnPropertyChanged(nameof(TomorrowPressure));
            OnPropertyChanged(nameof(DayAfterTomorrowPressure));

            CurrentWindspeed = Math.Round(weatherData.Today.Windspeed, 2) + " km/h";
            TomorrowWindspeed = Math.Round(weatherData.Tomorrow.Windspeed, 2) + " km/h";
            DayAfterTomorrowWindspeed = Math.Round(weatherData.DayAfterTomorrow.Windspeed, 2) + " km/h";
            OnPropertyChanged(nameof(CurrentWindspeed));
            OnPropertyChanged(nameof(TomorrowWindspeed));
            OnPropertyChanged(nameof(DayAfterTomorrowWindspeed));

            CurrentWindDirection = ConvertDegreesIntoNautic(weatherData.Today.WindDirection);
            TomorrowWindDirection = ConvertDegreesIntoNautic(weatherData.Tomorrow.WindDirection);
            DayAfterTomorrowWindDirection = ConvertDegreesIntoNautic(weatherData.DayAfterTomorrow.WindDirection);
            OnPropertyChanged(nameof(CurrentWindDirection));
            OnPropertyChanged(nameof(TomorrowWindDirection));
            OnPropertyChanged(nameof(DayAfterTomorrowWindDirection));

            CurrentWeatherCode = ConvertWeatherCodeToText(weatherData.Today.WeatherCode);
            TomorrowWeatherCode = ConvertWeatherCodeToText(weatherData.Tomorrow.WeatherCode);
            DayAfterTomorrowWeatherCode = ConvertWeatherCodeToText(weatherData.DayAfterTomorrow.WeatherCode);
            OnPropertyChanged(nameof(CurrentWeatherCode));
            OnPropertyChanged(nameof(TomorrowWeatherCode));
            OnPropertyChanged(nameof(DayAfterTomorrowWeatherCode));

            CurrentPrecipitationProbability = weatherData.Today.PrecipitationProbability + " % Niederschlag";
            TomorrowPrecipitationProbability = weatherData.Tomorrow.PrecipitationProbability + " % Niederschlag";
            DayAfterTomorrowPrecipitationProbability = weatherData.DayAfterTomorrow.PrecipitationProbability + " % Niederschlag";
            OnPropertyChanged(nameof(CurrentPrecipitationProbability));
            OnPropertyChanged(nameof(TomorrowPrecipitationProbability));
            OnPropertyChanged(nameof(DayAfterTomorrowPrecipitationProbability));

            CurrentWeatherCodeGlyph = ConvertWeatherCodeIntoGlyph(weatherData.Today.WeatherCode);
            TomorrowWeatherCodeGlyph = ConvertWeatherCodeIntoGlyph(weatherData.Tomorrow.WeatherCode);
            DayAfterTomorrowWeatherCodeGlyph = ConvertWeatherCodeIntoGlyph(weatherData.DayAfterTomorrow.WeatherCode);
            OnPropertyChanged(nameof(CurrentWeatherCodeGlyph));
            OnPropertyChanged(nameof(TomorrowWeatherCodeGlyph));
            OnPropertyChanged(nameof(DayAfterTomorrowWeatherCodeGlyph));
            //LoadTomorrowWeatherIcon(ConvertWeatherCodeIntoGlyph(weatherData.Tomorrow.WeatherCode)).ConfigureAwait(false);
            //LoadDayAfterTomorrowWeatherIcon(ConvertWeatherCodeIntoGlyph(weatherData.DayAfterTomorrow.WeatherCode)).ConfigureAwait(false);
        }

        internal string ConvertDegreesIntoNautic(int windDegrees)
        {
            switch (windDegrees)
            {
                case > 315:
                    return "Nordwesten";
                case > 270:
                    return "Westen";
                case > 225:
                    return "Südwesten";
                case > 180:
                    return "Süden";
                case > 135:
                    return "Südosten";
                case > 90:
                    return "Osten";
                case > 45:
                    return "Nordosten";
                default:
                    return "Norden";
            }
        }

        internal string ConvertWeatherCodeToText(WeatherCode code)
        {
            switch (code)
            {
                case WeatherCode.CLEAR_SKY:
                    return "Klar";
                case WeatherCode.MAINLY_CLEAR:
                    return "Überwiegend klar";
                case WeatherCode.PARTLY_CLOUDY:
                    return "Teilweise bewölkt";
                case WeatherCode.OVERCAST:
                    return "Bewölkt";
                case WeatherCode.FOG:
                    return "Nebel";
                case WeatherCode.DEPOSITING_RIME_FOG:
                    return "Gefrierender Nebel";
                case WeatherCode.DRIZZLE_LIGHT:
                    return "Leichter Nieselregen";
                case WeatherCode.DRIZZLE_MODERATE:
                    return "Nieselregen";
                case WeatherCode.DRIZZLE_DENSE:
                    return "Starker Nieselregen";
                case WeatherCode.FREEZING_DRIZZLE_LIGHT:
                    return "Gefrierender Nieselregen";
                case WeatherCode.FREEZING_DRIZZLE_DENSE:
                    return "Dichter gefrierender Nieselregen";
                case WeatherCode.RAIN_SLIGHTLY:
                    return "Leichter Regen";
                case WeatherCode.RAIN_LIGHT:
                    return "Regen";
                case WeatherCode.RAIN_HEAVY:
                    return "Starkregen";
                case WeatherCode.FREEZING_RAIN_LIGHT:
                    return "Gefrierender Regen";
                case WeatherCode.FREEZING_RAIN_HEAVY:
                    return "Starker gefrierender Regen";
                case WeatherCode.SNOW_FALL_LIGHT:
                    return "Leichter Schneefall";
                case WeatherCode.SNOW_FALL_MODERATE:
                    return "Schnee";
                case WeatherCode.SNOW_FALL_HEAVY:
                    return "Starker Schneefall";
                case WeatherCode.SNOW_GRAINS:
                    return "Hagel";
                case WeatherCode.RAIN_SHOWERS_SLIGHT:
                    return "Leichter Regenschauer";
                case WeatherCode.RAIN_SHOWERS_MODERATE:
                    return "Regenschauer";
                case WeatherCode.RAIN_SHOWERS_VIOLENT:
                    return "Starke Regenschauer";
                case WeatherCode.SNOW_SHOWERS_SLIGHT:
                    return "Leichter Schneefall";
                case WeatherCode.SNOW_SHOWERS_HEAVY:
                    return "Heftiger Schneefall";
                case WeatherCode.THUNDERSTORM_SLIGHT:
                    return "Gewitter";
                case WeatherCode.THUNDERSTORM_SLIGHT_HAIL:
                    return "Gewitter mit Hagel";
                case WeatherCode.THUNDERSTORM_HEAVY_HAIL:
                    return "Starke Gewitter mit Hagel";
                default:
                    return string.Empty;
            }
        }

        internal string ConvertWeatherCodeIntoGlyph(WeatherCode code)
        {
            switch (code)
            {
                case WeatherCode.CLEAR_SKY:
                    return "";
                case WeatherCode.MAINLY_CLEAR:
                    return "";
                case WeatherCode.PARTLY_CLOUDY:
                    return "";
                case WeatherCode.OVERCAST:
                    return "";
                case WeatherCode.FOG:
                    return "";
                case WeatherCode.DEPOSITING_RIME_FOG:
                    return "";
                case WeatherCode.DRIZZLE_LIGHT:
                    return "";
                case WeatherCode.DRIZZLE_MODERATE:
                    return "";
                case WeatherCode.DRIZZLE_DENSE:
                    return ""; ;
                case WeatherCode.FREEZING_DRIZZLE_LIGHT:
                    return "";
                case WeatherCode.FREEZING_DRIZZLE_DENSE:
                    return "";
                case WeatherCode.RAIN_SLIGHTLY:
                    return "";
                case WeatherCode.RAIN_LIGHT:
                    return "";
                case WeatherCode.RAIN_HEAVY:
                    return "";
                case WeatherCode.FREEZING_RAIN_LIGHT:
                    return "";
                case WeatherCode.FREEZING_RAIN_HEAVY:
                    return "";
                case WeatherCode.SNOW_FALL_LIGHT:
                    return "";
                case WeatherCode.SNOW_FALL_MODERATE:
                    return "";
                case WeatherCode.SNOW_FALL_HEAVY:
                    return "";
                case WeatherCode.SNOW_GRAINS:
                    return "";
                case WeatherCode.RAIN_SHOWERS_SLIGHT:
                    return "";
                case WeatherCode.RAIN_SHOWERS_MODERATE:
                    return "";
                case WeatherCode.RAIN_SHOWERS_VIOLENT:
                    return "";
                case WeatherCode.SNOW_SHOWERS_SLIGHT:
                    return "";
                case WeatherCode.SNOW_SHOWERS_HEAVY:
                    return "";
                case WeatherCode.THUNDERSTORM_SLIGHT:
                    return "";
                case WeatherCode.THUNDERSTORM_SLIGHT_HAIL:
                    return "";
                case WeatherCode.THUNDERSTORM_HEAVY_HAIL:
                    return "";
                default:
                    return "";
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}